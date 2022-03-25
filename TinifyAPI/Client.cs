using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TinifyAPI
{
    using Method = HttpMethod;

    public class Client : IDisposable
    {
        public static readonly Uri ApiEndpoint = new Uri("https://api.tinify.com");

        public static readonly ushort RetryCount = 1;
        public static readonly ushort RetryDelay = 500;

        public static readonly string UserAgent = Internal.Platform.UserAgent;

        HttpClient client;

        public delegate void HttpSendProgress(string mark, int progress, long? totalSize);

        public HttpSendProgress SendProgress;

        public delegate void HttpReceiveProgress(string mark, int progress, long? totalSize);

        public HttpReceiveProgress ReceiveProgress;


        private void ProgressMessageHandler_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            if (sender is HttpRequestMessage requestMessage)
            {
                if (requestMessage.Headers.TryGetValues("mark", out IEnumerable<string> marks))
                {
                    List<string> list = marks.ToList();
                    if (list.Any() && SendProgress != null)
                    {
                        SendProgress(list.First(), e.ProgressPercentage, e.TotalBytes);
                    }
                }
            }
        }

        private void ProgressMessageHandler_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
            if (sender is HttpRequestMessage requestMessage)
            {
                if (requestMessage.Headers.TryGetValues("mark", out IEnumerable<string> marks))
                {
                    List<string> list = marks.ToList();
                    if (list.Any() && list.First().StartsWith("location:"))
                    {
                        if (ReceiveProgress != null)
                        {
                            ReceiveProgress(list.First().Substring("location:".Length), e.ProgressPercentage,
                                e.TotalBytes);
                        }
                    }
                }
            }
        }

        public Client(string key, string appIdentifier = null, string proxy = null)
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = Internal.SSL.ValidationCallback
            };

            if (proxy != null)
            {
                handler.Proxy = new Internal.Proxy(proxy);
                handler.UseProxy = true;
            }

            ProgressMessageHandler processMessageHandler = new ProgressMessageHandler(handler);
            processMessageHandler.HttpSendProgress += ProgressMessageHandler_HttpSendProgress;
            processMessageHandler.HttpReceiveProgress += ProgressMessageHandler_HttpReceiveProgress;

            client = new HttpClient(processMessageHandler)
            {
                BaseAddress = ApiEndpoint,
                Timeout = Timeout.InfiniteTimeSpan,
            };


            var userAgent = UserAgent;
            if (appIdentifier != null)
            {
                userAgent = userAgent + " " + appIdentifier;
            }

            client.DefaultRequestHeaders.Add("User-Agent", userAgent);

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("api:" + key));
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
        }

        public Task<HttpResponseMessage> Request(Method method, string url)
        {
            return Request(method, new Uri(url, UriKind.Relative));
        }

        public Task<HttpResponseMessage> Request(Method method, string url, byte[] body, string mark)
        {
            return Request(method, new Uri(url, UriKind.Relative), body, mark);
        }

        public Task<HttpResponseMessage> Request(Method method, string url, Dictionary<string, object> options,
            string mark)
        {
            return Request(method, new Uri(url, UriKind.Relative), options, mark);
        }

        public Task<HttpResponseMessage> Request(Method method, Uri url, byte[] body, string mark)
        {
            return Request(method, url, new StreamContent(new MemoryStream(body)), mark);
        }

        public Task<HttpResponseMessage> Request(Method method, Uri url, Dictionary<string, object> options,
            string mark)
        {
            if (method == HttpMethod.Get && options.Count == 0)
            {
                return Request(method, url, mark: mark);
            }
            else
            {
                var json = JsonConvert.SerializeObject(options);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                return Request(method, url, body, mark);
            }
        }

        public async Task<HttpResponseMessage> Request(Method method, Uri url, HttpContent body = null, string mark = null)
        {
            for (short retries = (short) RetryCount; retries >= 0; retries--)
            {
                if (retries < RetryCount)
                {
                    await Task.Delay(RetryDelay);
                }

                var request = new HttpRequestMessage(method, url)
                {
                    Content = body
                };
                request.Headers.Remove("mark");
                if (mark != null)
                    request.Headers.Add("mark", mark);


                HttpResponseMessage response;
                try
                {
                    response = await client.SendAsync(request).ConfigureAwait(false);
                }
                catch (OperationCanceledException err)
                {
                    if (retries > 0) continue;
                    throw new ConnectionException("Timeout while connecting", err);
                }
                catch (System.Exception err)
                {
                    if (err.InnerException != null)
                    {
                        err = err.InnerException;
                    }

                    if (retries > 0) continue;
                    throw new ConnectionException("Error while connecting: " + err.Message, err);
                }

                if (response.Headers.Contains("Compression-Count"))
                {
                    var compressionCount = response.Headers.GetValues("Compression-Count").First();
                    uint parsed;
                    if (uint.TryParse(compressionCount, out parsed))
                    {
                        Tinify.CompressionCount = parsed;
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                var data = new {message = "", error = ""};
                try
                {
                    data = JsonConvert.DeserializeAnonymousType(
                        await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                        data
                    );
                }
                catch (System.Exception err)
                {
                    data = new
                    {
                        message = "Error while parsing response: " + err.Message,
                        error = "ParseError"
                    };
                }

                if (retries > 0 && (uint) response.StatusCode >= 500) continue;
                throw TinifyException.Create(data.message, data.error, (uint) response.StatusCode);
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }
    }
}
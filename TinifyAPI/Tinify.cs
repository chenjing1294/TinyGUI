using System.Net.Http;
using System.Threading.Tasks;

namespace TinifyAPI
{
    using Method = HttpMethod;

    public class Tinify
    {
        private static object mutex = new object();
        private static Client client;

        private static string key;
        private static string appIdentifier;
        private static string proxy;

        public static string Key
        {
            get { return key; }

            set
            {
                key = value;
                ResetClient();
            }
        }

        public static string AppIdentifier
        {
            get { return appIdentifier; }

            set
            {
                appIdentifier = value;
                ResetClient();
            }
        }

        public static string Proxy
        {
            get { return proxy; }

            set
            {
                proxy = value;
                ResetClient();
            }
        }

        private static void ResetClient()
        {
            if (client != null)
            {
                client.Dispose();
            }

            client = null;
        }

        public static uint? CompressionCount { get; set; }

        public static Client Client
        {
            get
            {
                if (key == null)
                {
                    throw new AccountException("Provide an API key with Tinify.Key = ...");
                }

                if (client != null)
                {
                    return client;
                }
                else
                {
                    lock (mutex)
                    {
                        if (client == null)
                        {
                            client = new Client(key, appIdentifier, proxy);
                        }
                    }

                    return client;
                }
            }
        }

        public static Task<Source> FromFile(string path)
        {
            return Source.FromFile(path);
        }

        public static Task<Source> FromBuffer(byte[] buffer, string mark)
        {
            return Source.FromBuffer(buffer, mark);
        }

        public static Task<Source> FromUrl(string url)
        {
            return Source.FromUrl(url);
        }

        public static async Task<bool> Validate()
        {
            try
            {
                await Client.Request(Method.Post, "/shrink").ConfigureAwait(false);
            }
            catch (AccountException err)
            {
                if (err.Status == 429)
                {
                    return true;
                }

                throw err;
            }
            catch (ClientException)
            {
                return true;
            }

            return false;
        }
    }
}
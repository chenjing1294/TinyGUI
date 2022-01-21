using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TinifyAPI
{
    using Method = HttpMethod;

    public class Source
    {
        public static async Task<Source> FromFile(string path)
        {
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var buffer = new MemoryStream())
            {
                await file.CopyToAsync(buffer).ConfigureAwait(false);
                return await FromBuffer(buffer.ToArray(), path).ConfigureAwait(false);
            }
        }

        public static async Task<Source> FromBuffer(byte[] buffer, string mark)
        {
            var response = await Tinify.Client.Request(Method.Post, "/shrink", buffer, mark).ConfigureAwait(false);
            var location = response.Headers.Location;
            return new Source(location);
        }

        public static async Task<Source> FromUrl(string url)
        {
            var body = new Dictionary<string, object>();
            body.Add("source", new {url = url});

            var response = await Tinify.Client.Request(Method.Post, "/shrink", body, url).ConfigureAwait(false);
            var location = response.Headers.Location;

            return new Source(location);
        }

        Uri url;
        Dictionary<string, object> commands;

        internal Source(Uri url, Dictionary<string, object> commands = null)
        {
            this.url = url;
            if (commands == null) commands = new Dictionary<string, object>();
            this.commands = commands;
        }

        public Source Preserve(params string[] options)
        {
            return new Source(url, mergeCommands("preserve", options));
        }

        public Source Resize(object options)
        {
            return new Source(url, mergeCommands("resize", options));
        }

        public async Task<ResultMeta> Store(object options, string mark)
        {
            var commands = mergeCommands("store", options);
            var response = await Tinify.Client.Request(Method.Post, url, commands, mark).ConfigureAwait(false);

            return new ResultMeta(response.Headers);
        }

        public async Task<Result> GetResult(string mark)
        {
            HttpResponseMessage response;
            if (commands.Count == 0)
            {
                response = await Tinify.Client.Request(Method.Get, url, mark: $"location:{mark}").ConfigureAwait(false);
            }
            else
            {
                response = await Tinify.Client.Request(Method.Post, url, commands, $"location:{mark}").ConfigureAwait(false);
            }

            var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return new Result(response.Headers, response.Content.Headers, body);
        }

        public async Task ToFile(string path, string mark)
        {
            await GetResult(mark).ToFile(path).ConfigureAwait(false);
        }

        public async Task<byte[]> ToBuffer(string mark)
        {
            return await GetResult(mark).ToBuffer().ConfigureAwait(false);
        }

        private Dictionary<string, object> mergeCommands(string key, object options)
        {
            var commands = new Dictionary<string, object>(this.commands);
            commands.Add(key, options);
            return commands;
        }
    }
}
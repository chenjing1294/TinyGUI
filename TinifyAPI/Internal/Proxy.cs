using System;
using System.Net;

namespace TinifyAPI.Internal
{
    internal class Proxy : IWebProxy
    {
        Uri uri { get; set; }

        public ICredentials Credentials { get; set; }

        public Proxy(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ConnectionException(
                    string.Format("Invalid proxy: cannot parse '{0}'", url)
                );
            }

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var user = uri.UserInfo.Split(':');
                Credentials = new NetworkCredential(user[0], user[1]);
            }
        }

        public Uri GetProxy(Uri destination)
        {
            return uri;
        }

        public bool IsBypassed(Uri host)
        {
            return host.IsLoopback;
        }
    }
}
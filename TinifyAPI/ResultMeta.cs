using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace TinifyAPI
{
    public class ResultMeta
    {
        protected HttpResponseHeaders meta;

        internal ResultMeta(HttpResponseHeaders meta)
        {
            this.meta = meta;
        }

        public uint? Width
        {
            get
            {
                uint value;
                IEnumerable<string> values;
                if (!meta.TryGetValues("Image-Width", out values))
                {
                    return null;
                }

                foreach (var header in values)
                {
                    if (uint.TryParse(header, out value))
                    {
                        return value;
                    }
                }
                return null;
            }
        }

        public uint? Height
        {
            get
            {
                uint value;
                IEnumerable<string> values;
                if (!meta.TryGetValues("Image-Height", out values))
                {
                    return null;
                }

                foreach (var header in values)
                {
                    if (uint.TryParse(header, out value))
                    {
                        return value;
                    }
                }
                return null;
            }
        }

        public Uri Location
        {
            get
            {
                return meta.Location;
            }
        }
    }
}

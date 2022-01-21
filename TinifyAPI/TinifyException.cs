namespace TinifyAPI
{
    public class TinifyException : System.Exception {
        internal static TinifyException Create(string message, string type, uint status) {
            if (status == 401 || status == 429)
            {
                return new AccountException(message, type, status);
            }
            else if (status >= 400 && status <= 499)
            {
                return new ClientException(message, type, status);
            }
            else if (status >= 500 && status <= 599)
            {
                return new ServerException(message, type, status);
            }
            else
            {
                return new TinifyException(message, type, status);
            }
        }

        public uint Status = 0;

        internal TinifyException() : base() {}

        internal TinifyException(string message, System.Exception err = null) : base(message, err) { }

        internal TinifyException(string message, string type, uint status) :
            base(message + " (HTTP " + status + "/" + type + ")")
        {
            this.Status = status;
        }
    }

    public class AccountException : TinifyException
    {
        internal AccountException() : base() {}

        internal AccountException(string message, System.Exception err = null) : base(message, err) { }

        internal AccountException(string message, string type, uint status) : base(message, type, status) { }
    }

    public class ClientException : TinifyException
    {
        internal ClientException() : base() {}

        internal ClientException(string message, System.Exception err = null) : base(message, err) { }

        internal ClientException(string message, string type, uint status) : base(message, type, status) { }
    }

    public class ServerException : TinifyException
    {
        internal ServerException() : base() {}

        internal ServerException(string message, System.Exception err = null) : base(message, err) { }

        internal ServerException(string message, string type, uint status) : base(message, type, status) { }
    }

    public class ConnectionException : TinifyException
    {
        internal ConnectionException() : base() {}

        internal ConnectionException(string message, System.Exception err = null) : base(message, err) { }

        internal ConnectionException(string message, string type, uint status) : base(message, type, status) { }
    }
}

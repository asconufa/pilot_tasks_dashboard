using System;

namespace Ascon.Pilot.Core
{
    /// <summary>
    /// Класс для работы со строкой подключения
    /// </summary>
    public static class ConnectionValidator
    {
        public const int DEFAULT_HTTP_PORT = 5545;

        /// <summary>
        /// Adds protocol to connection url if required
        /// </summary>
        /// <param name="connectionUrl">Connection url</param>
        /// <returns>Normalized connection url</returns>
        public static string NormalizeUri(string connectionUrl)
        {
            Uri uri;
            var res = Uri.TryCreate(connectionUrl, UriKind.Absolute, out uri);

            //строка вида experiment:5000 является валидной, при этом протоколом будет experiment, что не верно
            //по-этому проверим хост. В этом случае он будет равен Unknown
            if (!res || Uri.CheckHostName(uri.Host) == UriHostNameType.Unknown)
                return "http" + "://" + connectionUrl;

            return connectionUrl;
        }

        public static bool Port80Specified(string url)
        {
            return url.Contains(":80");
        }

        public static bool IsValidUrlToDatabase(string connectionUrl)
        {
            Uri uri;
            return TryCreateConnectionUrl(connectionUrl, out uri) && uri.AbsolutePath != "/" && uri.Segments.Length <= 2;
        }

        public static bool IsValidUrlToServer(string connectionUrl)
        {
            Uri uri;
            return TryCreateConnectionUrl(connectionUrl, out uri) && uri.Segments.Length == 1;
        }

        private static bool TryCreateConnectionUrl(string connectionUrl, out Uri uri)
        {
            connectionUrl = NormalizeUri(connectionUrl);

            if (!Uri.TryCreate(connectionUrl, UriKind.Absolute, out uri))
                return false;

            if (connectionUrl.Contains(" "))
                return false;

            if (!uri.Scheme.Equals("http"))
                return false;

            return true;
        }
    }
}

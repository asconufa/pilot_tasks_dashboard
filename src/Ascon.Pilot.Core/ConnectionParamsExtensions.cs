using System;

namespace Ascon.Pilot.Core
{
    public static class ConnectionParamsExtensions
    {
        public static Uri Url(this ConnectionParams connectParams)
        {
            string normalizedServerUri = ConnectionValidator.NormalizeUri(connectParams.Server);
            return new Uri(normalizedServerUri.ToLower());
        }

        public static bool IsValid(this ConnectionParams connectParams)
        {
            if (connectParams == null)
                return false;

            bool baseConfigOk = !string.IsNullOrEmpty(connectParams.Server) && !String.IsNullOrEmpty(connectParams.UserName);
            bool proxyOk = (!String.IsNullOrEmpty(connectParams.Proxy.Url) && connectParams.Proxy.Port != 0);

            //не использовать прокси
            if (!connectParams.Proxy.IsRequired)
                return baseConfigOk;

            //использовать прокси без авторизации
            if (connectParams.Proxy.IsRequired && !connectParams.Proxy.IsAuthRequired)
                return baseConfigOk && proxyOk;

            //использовать прокси с авторизацией
            if (connectParams.Proxy.IsRequired && connectParams.Proxy.IsAuthRequired)
                return baseConfigOk && !String.IsNullOrEmpty(connectParams.Proxy.UserName);

            //По умолчанию
            return false;
        }

        public static string ServerName(this ConnectionParams connectParams)
        {
            if (string.IsNullOrEmpty(connectParams.Server))
                return string.Empty;
            return Url(connectParams).Host.ToLower();
        }

        public static string Db(this ConnectionParams connectParams)
        {
            if (string.IsNullOrEmpty(connectParams.Server))
                return string.Empty;

            return Uri.UnescapeDataString(Url(connectParams).AbsolutePath.TrimStart('/'));
        }
    }
}

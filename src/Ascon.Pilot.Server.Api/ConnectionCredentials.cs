using System;
using System.Net;
using System.Net.Http;
using System.Security;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api
{
    public class ConnectionCredentials
    {
        public Uri ServerUrl { get; private set; }
        public bool PortSpecifiedByUser { get; private set; }
        public string Username { get; private set; }
        public SecureString Password { get; private set; }

        public bool UseWindowsAuth
        {
            get { return !String.IsNullOrEmpty(Username) && (Username.Contains("\\") || Username.Contains("@")); }
        }

        public string DatabaseName
        {
            get { return ServerUrl != null ? Uri.UnescapeDataString(ServerUrl.AbsolutePath.TrimStart('/')) : null; }
        }

        public string ProtectedPassword
        {
            get { return Password.ConvertToUnsecureString().EncryptAes(); }
        }

        public Uri PersonUrl
        {
            get
            {
                var uri = new UriBuilder(new Uri(ServerUrl, DatabaseName));
                uri.UserName = Uri.EscapeDataString(Username);
                return uri.Uri;
            }
        }

        public bool ProxyRequired { get; private set; }
        /*public string ProxyUrl { get; private set; }
        public int ProxyPort { get; private set; }
        public bool ProxyAuthRequired { get; private set; }
        public string ProxyUserName { get; private set; }
        public SecureString ProxyPassword { get; private set; }   */

        public static ConnectionCredentials GetConnectionCredentials(ConnectionParams connectionParams)
        {
            var credentials = new ConnectionCredentials
            {
                Password = connectionParams.Password,
                ServerUrl = connectionParams.Url(),
                PortSpecifiedByUser = ConnectionValidator.Port80Specified(connectionParams.Server),
                Username = connectionParams.UserName,
                /*ProxyAuthRequired = connectionParams.Proxy.IsAuthRequired,
                ProxyPassword = connectionParams.Proxy.Password,
                ProxyPort = connectionParams.Proxy.Port,
                ProxyRequired = connectionParams.Proxy.IsRequired,
                ProxyUrl = connectionParams.Proxy.Url,
                ProxyUserName = connectionParams.Proxy.UserName*/
            };

            return credentials;
        }

        public static ConnectionCredentials GetConnectionCredentials(string serverUrl, string username, SecureString password)
        {
            var credentials = new ConnectionCredentials
            {
                ServerUrl = new Uri(serverUrl),
                PortSpecifiedByUser = ConnectionValidator.Port80Specified(serverUrl),
                ProxyRequired = false,
                Username = username,
                Password = password,
            };

            return credentials;
        }
    }

    public static class ConnectionCredentialsEx
    {
        public static string GetConnectionString(this ConnectionCredentials connectionCredentials)
        {
            var connectionUrl = connectionCredentials.ServerUrl;
            var uri = new UriBuilder {Host = connectionUrl.Host, Scheme = "http"};

            if (String.IsNullOrEmpty(connectionUrl.GetComponents(UriComponents.Port, UriFormat.Unescaped)) && !connectionCredentials.PortSpecifiedByUser)
                uri.Port = ConnectionValidator.DEFAULT_HTTP_PORT;
            else
                uri.Port = connectionUrl.Port;

            return uri.Uri.ToString().TrimEnd('/');
        }

        public static IWebProxy GetConnectionProxy(this ConnectionCredentials connectionCredentials)
        {
            throw new NotImplementedException("WebProxy");
            /*if (!connectionCredentials.ProxyRequired)
                return null;
            
            var proxy = new WebProxy(connectionCredentials.ProxyUrl, connectionCredentials.ProxyPort);
            if (connectionCredentials.ProxyAuthRequired)
            {
                if(DomainHelper.IsDomainUser(connectionCredentials.ProxyUserName))
                    proxy.UseDefaultCredentials = true;
                else
                    proxy.Credentials = new NetworkCredential(connectionCredentials.ProxyUserName, connectionCredentials.ProxyPassword.ConvertToUnsecureString());
            }
            return proxy;*/
        }
    }
}
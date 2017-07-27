using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Ascon.Pilot.Transport
{
    internal class Context
    {
        private readonly HttpContext _httpContext;

        public Context(HttpContext httpContext)
        {
            this._httpContext = httpContext;
            //Для обработки запросов от Google Chrome. Не влияет на остальные запросы
            _httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            _httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
        }

        public string Method 
        {
            get { return _httpContext.Request.ToUri().AbsoluteUri; }
        }

        public string GetCookie(string name)
        {
            if (!_httpContext.Request.Cookies.ContainsKey(name))
                return string.Empty;
            return _httpContext.Request.Cookies[name];
        }

        public void SetCookie(string name, string value)
        {
            _httpContext.Response.Cookies.Append(name, value);
        }

        public void SetCookie(string name, string value, string path)
        {
            _httpContext.Response.Cookies.Append(name, value, new CookieOptions{Path = path});
        }

        public void SetCookie(string name, string value, string path, string domain)
        {
            _httpContext.Response.Cookies.Append(name, value, new CookieOptions { Path = path, Domain = domain });
        }

        public byte[] GetInput()
        {
            return _httpContext.Request.Body.ToByteArray();
        }

        public byte[] GetKeys()
        {
            return Encoding.Unicode.GetBytes(_httpContext.Request.QueryString.ToString());
        }

        public void BadSession()
        {
            _httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            _httpContext.Response.Body.Dispose();
        }

        public void Send()
        {
            _httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            _httpContext.Response.Body.Dispose();
        }

        public void Send(string message)
        {
            _httpContext.Response.ContentType = "text/plain";
            _httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            _httpContext.Response.SendAndClose(Encoding.UTF8.GetBytes(message), false);
        }

        public void Send(byte[] data)
        {
            _httpContext.Response.ContentType = "application/octet-stream";
            _httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            _httpContext.Response.ContentLength = data.Length;
            _httpContext.Response.SendAndClose(data, false);
        }

        public void Send(Exception ex)
        {
            var response = GetExceptionData(ex);
            _httpContext.Response.ContentType = "application/octet-stream";
            _httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            _httpContext.Response.SendAndClose(response, false);
        }

        static byte[] GetExceptionData(Exception exception)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    var serializedEx = JsonConvert.SerializeObject(exception);
                    var buffer = Encoding.Unicode.GetBytes(serializedEx);
                    stream.Write(buffer, 0, buffer.Length);
                    return stream.ToArray();
                }
                catch (Exception e)
                {
                    return GetExceptionData(
                        new InvalidOperationException(
                            String.Format("Server error getting exception info: {0}", e.Message)
                        )
                    );
                }
            }
        }
    }
}

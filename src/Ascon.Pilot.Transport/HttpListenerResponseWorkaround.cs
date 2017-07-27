using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Microsoft.AspNetCore.Http;

namespace Ascon.Pilot.Transport
{
    public static class HttpListenerResponseWorkaround
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
       // private static readonly bool _isNet45OrNewer;
        static HttpListenerResponseWorkaround()
        {
         //   _isNet45OrNewer = Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        public static async void SendAndClose(this HttpResponse response, byte[] data, bool willBlock)
        {
            // [Temproary commenting out the code for the test purposes]
            //if (_isNet45OrNewer)
            //{
            //    response.Close(data, willBlock);
            //}
            //else
            if (willBlock)
            {
                using (response.Body)
                {
                    response.Body.Write(data, 0, data.Length);
                }
            }
            else
            {
                using (response.Body)
                {
                    await response.Body.WriteAsync(data, 0, data.Length);
                }
            }
        }

        //private static void NonBlockingCloseCallback(IAsyncResult asyncResult)
        //{
        //    var response = (HttpResponse)asyncResult.AsyncState;
        //    try
        //    {
        //        var stream = response.OutputStream;
        //        stream.EndWrite(asyncResult);
        //    }
        //    catch (Win32Exception)
        //    {
        //    }
        //    // workaround of the issue found in 4.0 and fixed in 4.5 and higher versions
        //    //https://connect.microsoft.com/VisualStudio/feedback/details/674591/httplistener-crashes-server-processes-if-a-response-fails-to-complete-after-close-has-been-called
        //    try
        //    {
        //        response.Body.Dispose();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        Logger.Error(ex, ex.Message);
        //    }
        //}

    }

}

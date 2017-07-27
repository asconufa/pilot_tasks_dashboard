using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Ascon.Pilot.Transport
{
    public interface ISessionRegistry
    {
        ISession NewSession(ISessionCallback callback, IMarshallingFactory marshallingFactory);
        void CloseSession(ISession session);
        void CloseCallback(ISession session, byte[][] unsentData);
        byte[][] RestoreCallback(ISession session);
    }

    public interface ISession
    {
        byte[] Call(byte[] request);
        string Token { get; }
    }

    public interface ISessionCallback
    {
        void Callback(byte[] data);
    }

    /*public class TransportServer: IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISessionRegistry _registry;
        private readonly int _callbackTimeout;
        private readonly int _sessionTimeout;
        private readonly HttpListener _listener = new HttpListener();
        private readonly ConcurrentDictionary<Guid, Session> _sessions = new ConcurrentDictionary<Guid, Session>();
        private volatile bool _listening;
        
        public TransportServer(ISessionRegistry registry, int callbackTimeout, int sessionTimeout)
        {
            _listener.IgnoreWriteExceptions = true;
            this._registry = registry;
            this._callbackTimeout = callbackTimeout;
            this._sessionTimeout = sessionTimeout;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(string url)
        {
            _listener.Prefixes.Add(url + "/");
            _listening = true;
            _listener.Start();
            _listener.BeginGetContext(ProcessGetContextAsync, _listener);
        }

        public void Stop()
        {
            if (_listening == false)
                return;
            _listening = false;
            CloseSessions();
            _listener.Close();
        }

        private void CloseSessions()
        {
            var closing = _sessions.Values;
            while (closing.Count > 0)
            {
                Parallel.ForEach(closing, session => CloseSession(session));
                closing = _sessions.Values;
            }
        }

        private void ProcessGetContextAsync(IAsyncResult result)
        {
            HttpListenerContext httpListenerContext = null;
            try
            {
                var httpListener = (HttpListener)result.AsyncState;
                if (_listening && httpListener.IsListening)
                    httpListener.BeginGetContext(ProcessGetContextAsync, httpListener);

                httpListenerContext = httpListener.EndGetContext(result);
            }
            catch 
            {
                return;
            }

            var context = new Context(httpListenerContext);
            try
            {
                ProcessRequest(context);
            }
            catch (HttpListenerException)
            {
                // We can do nothig here.
            }
            catch (Exception ex)
            {
                Logger.Info("ProcessRequest error", ex);
                try
                {
                    context.Send(ex);
                }
                catch (Exception exception)
                {
                    Logger.Error("ProcessRequest Send error", exception);
                }
            }
        }

        private Session GetSession(Context context)
        {
            Guid id;
            var cookie = context.GetCookie("SID");
            if (string.IsNullOrEmpty(cookie))
                return null;
            if (Guid.TryParse(cookie, out id) == false)
                return null;
            Session session;
            if (_sessions.TryGetValue(id, out session) == false)
                return null;
            return session;
        }

        private void ProcessRequest(Context context)
        {
            switch (context.Method)
            {
                case Method.CONNECT:
                    DoConnect(context);
                    break;
                case Method.CALL:
                    DoCall(context);
                    break;
                case Method.CALLBACK:
                    DoCallback(context);
                    break;
                case Method.DISCONNECT:
                    DoDisconnect(context);
                    break;
                case Method.ROOT:
                    DoPing(context);
                    break;
                case Method.WEB_CONNECT:
                    DoWebConnect(context);
                    break;
                case Method.WEB_CALL:
                    DoCall(context);
                    break;
                case Method.WEB_FILE:
                    DoFile(context);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown request [{0}]", context.Method));
            }
        }

        private void DoPing(Context context)
        {
            context.Send(string.Format("Pilot-Server_v{0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        private void DoConnect(Context context)
        {
            var serviceFactory = new MarshallingFactory();

            var session = new Session(_registry, _callbackTimeout, _sessionTimeout, CloseSession, serviceFactory);
            _sessions.TryAdd(session.Id, session);
            context.SetCookie("SID", session.Id.ToString(), "/");
            context.Send();
        }

        private void DoCall(Context context)
        {
            var session = GetSession(context);
            if (session == null)
            {
                context.BadSession();
                return;
            }
            session.ProcessCallRequest(context);
        }

        private void DoCallback(Context context)
        {
            var session = GetSession(context);
            if (session == null)
            {
                context.BadSession();
                return;
            }
            session.ProcessCallbackRequest(context);
        }

        private void DoDisconnect(Context context)
        {
            var session = GetSession(context);
            if (session != null)
            {
                CloseSession(session);
            }
            context.Send();
        }

        private void CloseSession(Session session)
        {
            session.Close(_registry);
            Session removed;
            _sessions.TryRemove(session.Id, out removed);
        }

        private void DoWebConnect(Context context)
        {
            var serviceFactory = new JsonMarshallingFactory();

            var session = new Session(_registry, _callbackTimeout, _sessionTimeout, CloseSession, serviceFactory);
            _sessions.TryAdd(session.Id, session);
            context.SetCookie("SID", session.Id.ToString(), "/", "");
            context.Send();
        }

        private void DoFile(Context context)
        {
            var session = GetSession(context);
            if (session == null)
            {
                context.BadSession();
                return;
            }
            session.ProcessGetRequest(context);
        }
    }*/
}

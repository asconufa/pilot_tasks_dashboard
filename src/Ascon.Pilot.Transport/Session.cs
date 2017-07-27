using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Ascon.Pilot.Transport
{
    class Session : ISessionCallback
    {
        private readonly ISession _session;
        private readonly int _callbackTimeout;
        private readonly int _sessionTimeout;
        private readonly Guid _id;
        private readonly CallbackQueue _callbackQueue = new CallbackQueue();
        private volatile Timer _callbackTimer;
        private readonly Timer _sessionTimer;
        private readonly OnSessionTimeout _onSessionTimeout;
        private long _isClosed = 0;
        private long _isCalling = 0;
        private readonly ISessionRegistry _registry;
        private string _token = string.Empty;

        public Guid Id { get { return _id; } }

        public delegate void OnSessionTimeout(Session session);

        public Session(ISessionRegistry registry, int callbackTimeout, int sessionTimeout, OnSessionTimeout onSessionTimeout, IMarshallingFactory marshallingFactory)
        {
            _callbackTimeout = callbackTimeout;
            _sessionTimeout = sessionTimeout;
            _onSessionTimeout = onSessionTimeout;
            _registry = registry;
            
            _id = Guid.NewGuid();
            _session = registry.NewSession(this, marshallingFactory);
            _sessionTimer = new Timer(SessionTimeout, null, sessionTimeout, Timeout.Infinite);
        }

        public void Close(ISessionRegistry registry)
        {
            var wasClosed = Interlocked.Increment(ref _isClosed);
            if (wasClosed > 1)
                return;

            while (Interlocked.Read(ref _isCalling) > 0)
                Thread.Sleep(1);

            registry.CloseCallback(_session, _callbackQueue.GetData());
            registry.CloseSession(_session);
            var response = _callbackQueue.FinishWaiting();
            SendBye(response.Context, response.Packet);
            
            _callbackTimer = null;
            _sessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void ProcessCallRequest(Context context)
        {
            if (Interlocked.Read(ref _isClosed) > 0)
                throw new TransportException("Session was closed");

            Interlocked.Increment(ref _isCalling);
            try
            {
                RestartSessionTimer();
                var request = context.GetInput();
                var response = _session.Call(request);
                context.Send(response);
                if (_token != _session.Token)
                    RestoreDumpCallbacks();
            }
            finally
            {
                Interlocked.Decrement(ref _isCalling);
            }
        }

        public void ProcessGetRequest(Context context)
        {
            if (Interlocked.Read(ref _isClosed) > 0)
                throw new TransportException("Session was closed");

            Interlocked.Increment(ref _isCalling);
            try
            {
                RestartSessionTimer();
                var request = context.GetKeys();
                var response = _session.Call(request);
                context.Send(response);
            }
            finally
            {
                Interlocked.Decrement(ref _isCalling);
            }
        }

        public void Callback(byte[] data)
        {
            StopCallbackTimer();
            var response = _callbackQueue.NewPacket(data);
            Send(response);
        }

        public void ProcessCallbackRequest(Context context)
        {
            if (Interlocked.Read(ref _isClosed) > 0)
                throw new TransportException("Session was closed");

            RestartSessionTimer();
            
            var response = _callbackQueue.NewContext(context);
            if (response.Context != context)
                StartCallbackTimer(context);
            Send(response);
        }

        private void RestoreDumpCallbacks()
        {
            _token = _session.Token;
            var dump = _registry.RestoreCallback(_session);
            
            if (dump == null)
                return;

            foreach (var data in dump)
            {
                _callbackQueue.AddPacket(data); 
            }
        }

        private void StartCallbackTimer(Context context)
        {
            _callbackTimer = new Timer(TimerCallback, context, _callbackTimeout, Timeout.Infinite);
        }

        private void StopCallbackTimer()
        {
            var oldTimer = _callbackTimer;
            _callbackTimer = null;
            if (oldTimer != null)
            {
                oldTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void TimerCallback(Object state)
        {
            var context = (Context)state;
            var response = _callbackQueue.ContextTimeout(context);
            Send(response);
        }

        private void Send(ResponseInfo response)
        {
            Send(response.Context, response.Packet);
        }

        private void Send(Context context, CallbackPacket packet)
        {
            if (context == null)
                return;
            try
            {
                if (packet == null)
                {
                    context.Send();
                }
                else
                {
                    context.SetCookie("CBID", packet.Id.ToString(CultureInfo.InvariantCulture));
                    context.Send(packet.Data);
                }
            }
            catch (HttpRequestException)
            {
                // We can do nothing here. 
            }
            catch (ObjectDisposedException)
            {
                // We can do nothing here.
            }
            catch (Exception ex)
            {
                try
                {
                    context.Send(ex);
                }
                catch (Exception)
                {
                    // We can do nothing here.  
                }
            }
        }

        private void SendBye(Context context, CallbackPacket packet)
        {
            if (context == null)
                return;
            try
            {
                context.BadSession();
            }
            catch (Exception)
            {
                // We can do nothing here.
            }
        }

        private void RestartSessionTimer()
        {
            _sessionTimer.Change(_sessionTimeout, Timeout.Infinite);
        }

        private void SessionTimeout(Object state)
        {
            _onSessionTimeout(this);
        }
    }
}

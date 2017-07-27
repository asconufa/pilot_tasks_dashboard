using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.Transport
{
    class CallbackPacket
    {
        private readonly long _id;
        private readonly byte[] _data;

        public long Id { get { return _id; } }
        public byte[] Data { get { return _data; } }

        public CallbackPacket(long id, byte[] data)
        {
            this._id = id;
            this._data = data;
        }
    }

    struct ResponseInfo
    {
        public Context Context;
        public CallbackPacket Packet;
        public ResponseInfo(Context context, CallbackPacket packet)
        {
            this.Context = context;
            this.Packet = packet;
        }
    }

    /// <summary>
    /// Thread safe
    /// </summary>
    class CallbackQueue
    {
        private object _locker = new Object();
        private Queue<CallbackPacket> _packets = new Queue<CallbackPacket>();
        private Context _waiting = null;
        private long _cbid = 0;

        public ResponseInfo FinishWaiting()
        {
            var response = new ResponseInfo(null, null);
            lock (_locker)
            {
                if (_waiting != null)
                {
                    response.Context = _waiting;
                    _waiting = null;
                    if (_packets.Count > 0)
                        response.Packet = _packets.Peek();
                }
            }
            return response;
        }

        public ResponseInfo ContextTimeout(Context context)
        {
            var response = new ResponseInfo(null, null);
            lock (_locker)
            {
                if (context == _waiting)
                {
                    response.Context = _waiting;
                    _waiting = null;
                    if (_packets.Count > 0)
                        response.Packet = _packets.Peek();
                }
            }
            return response;
        }

        public void AddPacket(byte[] data)
        {
            lock (_locker)
            {
                _packets.Enqueue(new CallbackPacket(++_cbid, data));
            }
        }

        public ResponseInfo NewPacket(byte[] data)
        {
            var response = new ResponseInfo(null, null);
            lock (_locker)
            {
                _packets.Enqueue(new CallbackPacket(++_cbid, data));
                if (_waiting != null)
                {
                    response.Context = _waiting;
                    _waiting = null;
                    response.Packet = _packets.Peek();
                }
            }
            return response;
        }

        public ResponseInfo NewContext(Context context)
        {
            var response = new ResponseInfo(null, null);
            lock (_locker)
            {
                RemoveAcceptedCallbacks(context);

                if (_packets.Count == 0)
                {
                    response.Context = _waiting;
                    _waiting = context;
                }
                else
                {
                    response.Packet = _packets.Peek();
                    if (_waiting == null)
                    {
                        response.Context = context;
                    }
                    else
                    {
                        response.Context = _waiting;
                        _waiting = context;
                    }
                }
            }
            return response;
        }

        private long GetCallbackId(Context context)
        {
            long result;
            string cookie = context.GetCookie("CBID");
            if (string.IsNullOrEmpty(cookie))
                return 0;
            if (long.TryParse(cookie, out result) == false)
                return 0;
            return result;
        }

        private void RemoveAcceptedCallbacks(Context context)
        {
            var lastAccepted = GetCallbackId(context);
            while (_packets.Count > 0 && _packets.Peek().Id <= lastAccepted)
                _packets.Dequeue();
        }

        public byte[][] GetData()
        {
            return _packets.Select(p => p.Data).ToArray();
        }
    }
}

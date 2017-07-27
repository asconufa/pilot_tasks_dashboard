using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Ascon.Pilot.Server.Api
{
    public interface IConnectionLostListener
    {
        void ConnectionLost(Exception ex = null);
    }

    public class FaultedEventArgs : EventArgs
    {
        public Exception FaultException { get; private set; }

        public FaultedEventArgs(Exception exception)
        {
            FaultException = exception;
        }
    }

    public class NetworkAddressChangedListener :IDisposable
    {
        private readonly IConnectionLostListener _connectionLostListener;
        private readonly ConnectionCredentials _credentials;

        public NetworkAddressChangedListener(IConnectionLostListener connectionLostListener, ConnectionCredentials credentials)
        {
            if (connectionLostListener == null) 
                throw new ArgumentNullException("connectionLostListener");
            if (credentials == null) 
                throw new ArgumentNullException("credentials");

            _connectionLostListener = connectionLostListener;
            _credentials = credentials;

            //NetworkChange.NetworkAddressChanged += NetworkAddressChanged;
            //NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
        }

        //private void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs networkAvailabilityEventArgs)
        //{
        //    if (!networkAvailabilityEventArgs.IsAvailable)
        //        _connectionLostListener.ConnectionLost();
        //}

        private void NetworkAddressChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!PingHost(_credentials.ServerUrl.Host))
                    _connectionLostListener.ConnectionLost();
            }
            catch (Exception)
            {
                _connectionLostListener.ConnectionLost();
            }
        }

        public static bool PingHost(string host, int timeout = 120)
        {
            var pingSender = new Ping();
            PingReply reply = pingSender.SendPingAsync(host, timeout).Result;
            if (reply == null)
                return false;
            if (reply.Status == IPStatus.Success)
                return true;
            return false;
        }

        public void Dispose()
        {
            //NetworkChange.NetworkAddressChanged -= NetworkAddressChanged;
            //NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;
        }
    }
}

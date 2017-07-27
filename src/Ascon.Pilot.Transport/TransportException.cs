using System;
using System.Runtime.Serialization;

namespace Ascon.Pilot.Transport
{
    //[Serializable]
    public class TransportException : Exception
    {
        public TransportException()
        {}

        public TransportException(string message) 
            : base(message)
        {}

        public TransportException(string message, Exception innerException)
            : base (message, innerException)
        {}
    }
}

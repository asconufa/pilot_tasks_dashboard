using System;
using System.Security;

namespace Ascon.Pilot.Core
{
    //[Serializable]
    public class ConnectionParams
    {
        public ConnectionParams()
        {
            Proxy = new ProxyParams();
            UserName = string.Empty;
            Password = new SecureString();
            Server = string.Empty;
        }
     
        public string UserName { get; set; }

        //[XmlIgnore]
        public SecureString Password { get; set; }

        //[XmlElement]
        public byte[] SafePassword
        {
            get { return Password.Protect(); }
            set { Password = value.Unprotect(); }
        }

        public string Server { get; set; }

        public ProxyParams Proxy { get; private set; }
    }
}

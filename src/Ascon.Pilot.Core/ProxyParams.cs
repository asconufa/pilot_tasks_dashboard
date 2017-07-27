using System;
using System.Security;
using System.Security.Cryptography;

namespace Ascon.Pilot.Core
{
    //[Serializable]
    public class ProxyParams
    {
        public bool IsRequired { get; set; }
        public string Url { get; set; }
        public int Port { get; set; }
        public bool IsAuthRequired { get; set; }
        public string UserName { get; set; }

        //[XmlIgnore]
        public SecureString Password { get; set; }

        //[XmlElement]
        public byte[] SafePassword
        {
            get { return Password.Protect(); }
            set { Password = value.Unprotect(); }
        }
    }
}

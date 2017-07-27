using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Ascon.Pilot.Core
{
    public static class SecureStringEx
    {
        public static SecureString ConvertToSecureString(this string str)
        {
            if (str == null)
                return new SecureString();

            var secureString = new SecureString();

            foreach (var c in str.ToCharArray())
                secureString.AppendChar(c);

            secureString.MakeReadOnly();
            return secureString;
        }

        public static string ConvertToUnsecureString(this SecureString secureString)
        {
            if (secureString == null)
                return null;

            IntPtr bstr;
            bstr = SecureStringMarshal.SecureStringToCoTaskMemUnicode(secureString);
#if (NET451)
            bstr = Marshal.SecureStringToBSTR(secureString);
#elif (DOTNET5_4)
            bstr = SecureStringMarshal.SecureStringToCoTaskMemUnicode(secureString);
#endif
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        public static bool AreEqual(this SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null)
                throw new ArgumentNullException("secureString1");

            if (secureString2 == null)
                throw new ArgumentNullException("secureString2");

            if (secureString1.Length != secureString2.Length)
                return false;

            var str1 = ConvertToUnsecureString(secureString1);
            var str2 = ConvertToUnsecureString(secureString2);
            return str1.Equals(str2); 
        }
    }
}

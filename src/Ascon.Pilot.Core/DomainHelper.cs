using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Ascon.Pilot.Core
{
    public static class DomainHelper
    {
        public static bool IsDomainUser(string userName)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            return windowsIdentity != null && userName != null &&
                String.Equals(userName, windowsIdentity.Name);
        }

        public static string GetCurrentDomainUserName()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            return windowsIdentity != null ? windowsIdentity.Name : string.Empty;
        }

        public static bool IsInDomain()
        {
            Win32.NetJoinStatus status;
            IntPtr pDomain;
            var result = Win32.NetGetJoinInformation(null, out pDomain, out status);
            if (pDomain != IntPtr.Zero)
            {
                Win32.NetApiBufferFree(pDomain);
            }
            if (result == Win32.ERROR_SUCCESS)
                return status == Win32.NetJoinStatus.NetSetupDomainName;

            return false;
        }

        class Win32
        {
            public const int ERROR_SUCCESS = 0;

            [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status);

            [DllImport("Netapi32.dll")]
            public static extern int NetApiBufferFree(IntPtr buffer);

            public enum NetJoinStatus
            {
                NetSetupUnknownStatus = 0,
                NetSetupUnjoined = 1,
                NetSetupWorkgroupName = 2,
                NetSetupDomainName = 3
            }

        }
    }
}

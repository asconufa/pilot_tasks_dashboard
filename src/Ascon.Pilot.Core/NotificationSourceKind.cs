using ProtoBuf;
using System;

namespace Ascon.Pilot.Core
{
    [ProtoContract(EnumPassthru = true)]
    [Flags]
    public enum NotificationSourceKind : byte
    {
        Object = 1,
        PilotStorage = 2,
        Task = 4
    }
}
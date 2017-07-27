using ProtoBuf;
using System;
using System.Linq;

namespace Ascon.Pilot.Core
{
    [ProtoContract(EnumPassthru = true)]
    public enum NotificationChangeKind : byte
    {
        Created = 0,
        Deleted = 1,
        Restored = 2,
        Renamed = 3,
        Moved = 4,
        Access = 5,
        Attribute = 6,
        File = 7,
        Signature = 8,
        Annotation = 9,
        Message = 10,
        StateAssigned = 11,
        StateInProgress = 12,
        StateOnValidation = 13,
        StateRevoked = 14,
        StateReturnedAfterValidation = 15,
        Attachment = 16,
        Title = 17,
        Description = 18
    }
}

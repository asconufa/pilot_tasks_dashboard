using System;
using System.Linq;

namespace Ascon.Pilot.Core.Numerators
{
    public class UnknownProvider : INumeratorKeywordProvider
    {
        public object GetValue(DObject obj, string keyword)
        {
            return "***";
        }
    }
}

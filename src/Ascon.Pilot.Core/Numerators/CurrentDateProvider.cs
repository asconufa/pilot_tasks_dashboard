using System;
using System.Linq;

namespace Ascon.Pilot.Core.Numerators
{
    public class CurrentDateProvider : INumeratorKeywordProvider
    {
        public const string CURRENT_DATE_KEYWORD = "CurrentDate";

        public object GetValue(DObject obj, string keyword)
        {
            if (keyword != CURRENT_DATE_KEYWORD)
                return null;

            return DateTime.Now;
        }
    }
}

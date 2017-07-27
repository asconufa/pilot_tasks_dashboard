using System;
using System.Linq;

namespace Ascon.Pilot.Core.Numerators
{
    public class AttributeKeywordProvider : INumeratorKeywordProvider
    {
        public object GetValue(DObject obj, string keyword)
        {
            DValue value;
            return obj.Attributes.TryGetValue(keyword, out value) ? value.Value : null;
        }
    }
}

using System.Linq;

namespace Ascon.Pilot.Core.Numerators
{
    public interface INumeratorKeywordProvider
    {
        object GetValue(DObject obj, string keyword);
    }
}
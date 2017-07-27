using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.Core.Numerators
{
    public class NumeratorKeywordProviderAggregator : INumeratorKeywordProvider
    {
        private readonly IEnumerable<INumeratorKeywordProvider> _providers;

        public NumeratorKeywordProviderAggregator(IEnumerable<INumeratorKeywordProvider> providers)
        {
            _providers = providers;
        }

        public object GetValue(DObject obj, string keyword)
        {
            return _providers.Select(x => x.GetValue(obj, keyword)).FirstOrDefault(x => x != null);
        }
    }
}
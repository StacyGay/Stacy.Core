using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public class ConfiguredObjectCacher : ObjectCacher
    {

        public ConfiguredObjectCacher(IDataSource dataSource)
            : base(dataSource)
        {
        }

        protected override string GetKey(object key)
        {
            var configuredKey = new
            {
                // TODO: Define configured parameters
                BaseKey = key
            };

            return base.GetKey(configuredKey);
        }

        protected override string GetBag(string bag = null)
        {
            var bags = new List<string>(); // TODO: Add default bag
            if (!string.IsNullOrEmpty(bag))
                bags.Add(bag);

            bags = bags.Distinct().ToList();

            return base.GetBag(string.Join(",", bags));
        }
    }
}

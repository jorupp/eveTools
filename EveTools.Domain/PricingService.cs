using System;
using System.Collections.Generic;
using System.Linq;
using EveAI;
using EveAI.Product;
using EveTools.Domain.Models;

namespace EveTools.Domain
{
    public class PricingService : IPricingService
    {
        private readonly DataCore _dataCore;
        private Lazy<IDictionary<int, ItemPricingStats>> _data;

        public PricingService(IPricingRepository pricingRepository, DataCore dataCore)
        {
            _dataCore = dataCore;
            _data = new Lazy<IDictionary<int, ItemPricingStats>>(() => pricingRepository.GetStats(dataCore.JitaId(), null).ToDictionary(i => i.typeId));
        }

        public decimal GetPrice(int typeId)
        {
            return GetPrice(_dataCore.FindProductType(typeId));
        }

        public decimal GetPrice(ProductType type)
        {
            if (null != type.Blueprint)
                return 0; // can't detect BPCs yet, so let's assume 0

            ItemPricingStats stats;
            if (!_data.Value.TryGetValue(_dataCore.GetIdForObject(type), out stats))
                return 0;
            if (null == stats.buy && null == stats.sell)
                return 0;
            if (null == stats.buy)
                return stats.sell.percentile;
            if (null == stats.sell)
                return stats.buy.percentile;
            return (stats.sell.percentile + stats.buy.percentile)/2;
        }
    }
}

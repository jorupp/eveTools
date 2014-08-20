using System;
using System.Collections.Generic;
using System.Linq;
using EveAI;
using EveAI.Map;
using EveAI.Product;

namespace EveTools.Domain
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize)
        {
            items = items.ToList();
            return Enumerable.Range(0, (int)Math.Ceiling(items.Count() / (decimal)chunkSize)).Select(i => items.Skip(i * chunkSize).Take(chunkSize));
        }

        public static SolarSystem Jita(this DataCore dataCore)
        {
            return dataCore.SolarSystems.Single(i => i.Name == "Jita");
        }

        public static int JitaId(this DataCore dataCore)
        {
            return dataCore.GetIdForObject(dataCore.Jita());
        }

        public static decimal GetPrice(this ProductType type, IPricingService pricingService)
        {
            return pricingService.GetPrice(type);
        }
    }
}

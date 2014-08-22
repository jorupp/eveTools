using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EveAI;
using EveAI.Live;
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

        public static IEnumerable<T> Recursive<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> recurse)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var child in recurse(item).Recursive(recurse))
                {
                    yield return child;
                }
            }
        }

        public static T Get<T>(this EveApi api) where T : EveApiBase, new()
        {
            var method =
                api.GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single(
                        i =>
                            i.Name == "UpdateEveApi" &&
                            i.GetParameters().Length == 1 &&
                            i.IsGenericMethodDefinition &&
                            i.GetGenericArguments().Length == 1);
            var t = new T();
            method.MakeGenericMethod(typeof(T)).Invoke(api, new object[] { t });
            return t;
        }
    }
}

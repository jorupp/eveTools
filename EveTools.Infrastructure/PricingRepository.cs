using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using EveAI.Live;
using EveTools.Domain;
using EveTools.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace EveTools.Infrastructure
{
    public class PricingRepository : IPricingRepository
    {
        private MongoCollection<PricingStats> _col;

        public PricingRepository(MongoDatabase database)
        {
            _col = database.GetCollection<PricingStats>("pricing");
        }

        public ItemPricingStats[] GetStats(int? systemId, int? regionId)
        {
            var query = 
                systemId.HasValue ? new QueryDocument("systemId", systemId.Value) :
                regionId.HasValue ? new QueryDocument("regionId", regionId.Value) :
                null;

            if(null == query)
                throw new ApplicationException("Invalid system/region specified");

            var data = _col.Find(query).SingleOrDefault();
            if (data == null || data.fetched < DateTime.UtcNow.AddHours(-1))
            {
                var filter =
                    systemId.HasValue ? "systemlimit=" + systemId.Value :
                    "regionlimit=" + regionId.Value;

                var baseUri = "http://api.eve-central.com/api/marketstat";
                // get new data
                var dataCore = new EveApi(true).EveApiCore;
                var itemIds = dataCore.ProductTypes.Where(i => i.MarketGroup != null).Select(i => dataCore.GetIdForObject(i)).ToArray();
                var tasks = itemIds.Chunk(100).Select(ids => Task.Run(async () =>
                {
                    var uri = baseUri + "?" + string.Join("&", ids.Select(i => "typeid=" + i)) + "&" + filter;
                    var http = new HttpClient();
                    var raw = await http.GetStringAsync(uri).ConfigureAwait(false);
                    var xml = XDocument.Parse(raw);
                    return xml.XPathSelectElements("//type").Select(ParseTypeElement).ToArray();
                })).ToArray();

                Task.WaitAll(tasks);

                var newData = new PricingStats
                {
                    id = data == null? ObjectId.GenerateNewId() : data.id,
                    systemId = systemId,
                    regionId = regionId,
                    fetched = DateTime.UtcNow,
                    items = tasks.SelectMany(i => i.Result).ToArray(),
                };

                var result = _col.Update(query, Update.Replace(newData), UpdateFlags.Upsert);
                if (!result.UpdatedExisting && null == result.Upserted)
                    throw new ApplicationException("Something broke :(");
                data = newData;
            }

            return data.items;
        }

        private ItemPricingStats ParseTypeElement(XElement typeElement)
        {
            var stats = new ItemPricingStats
            {
                typeId = int.Parse(typeElement.Attribute("id").Value),
                buy = ParseDetailElement(typeElement.Element("buy")),
                sell = ParseDetailElement(typeElement.Element("sell")),
                all = ParseDetailElement(typeElement.Element("all")),
            };

            return stats;
        }

        private ItemPricingStatsDetail ParseDetailElement(XElement detailElement)
        {
            return new ItemPricingStatsDetail()
            {
                volume = long.Parse(detailElement.Element("volume").Value),
                avg = decimal.Parse(detailElement.Element("avg").Value),
                max = decimal.Parse(detailElement.Element("max").Value),
                min = decimal.Parse(detailElement.Element("min").Value),
                stddev = decimal.Parse(detailElement.Element("stddev").Value),
                median = decimal.Parse(detailElement.Element("median").Value),
                percentile = decimal.Parse(detailElement.Element("percentile").Value),
            };
        }

        public class PricingStats
        {
            public ObjectId id { get; set; }
            public int? systemId { get; set; }
            public int? regionId { get; set; }
            public DateTime fetched { get; set; }
            public ItemPricingStats[] items { get; set; }
        }
    }

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize)
        {
            items = items.ToList();
            return Enumerable.Range(0, (int)Math.Ceiling(items.Count()/(decimal)chunkSize)).Select(i => items.Skip(i * chunkSize).Take(chunkSize));
        } 
    }
}

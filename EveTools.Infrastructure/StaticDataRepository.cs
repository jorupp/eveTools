using System.Collections.Generic;
using System.Linq;
using EveTools.Domain;
using EveTools.Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveTools.Infrastructure
{
    public class StaticDataRepository : IStaticDataRepository
    {
        private MongoCollection<BsonDocument> _col;

        public StaticDataRepository(MongoDatabase database)
        {
            _col = database.GetCollection("staticData");
        }

        public BlueprintMaterialInfo[] GetBlueprintMaterialInfo()
        {
            var query = new QueryDocument("table", "ramBlueprints");

            var data = _col.Find(query).Single();

            var bps = data["data"].AsBsonDocument;
            return bps.Names.Select(i =>
            {
                var activities = bps[i].AsBsonArray[1].AsBsonDocument;
                return new BlueprintMaterialInfo()
                {
                    TypeId = int.Parse(i),
                    ActivityMaterialInfo = activities.Names.Select(ii => new BlueprintActivityMaterialInfo()
                    {
                        ActivityId = int.Parse(ii),
                        Time = activities[ii].AsBsonDocument["time"].AsInt32,
                        Materials = Parse(activities[ii].AsBsonDocument.Contains("materials") ? activities[ii].AsBsonDocument["materials"].AsBsonDocument : null),
                        Products = Parse(activities[ii].AsBsonDocument.Contains("products") ?activities[ii].AsBsonDocument["products"].AsBsonDocument : null),

                    }).ToDictionary(ii => ii.ActivityId)
                };
            }).ToArray();
        }

        private Dictionary<int, int> Parse(BsonDocument doc)
        {
            if(null == doc)
                return new Dictionary<int, int>();
            return doc.Names.Select(i => new { key = int.Parse(i), value = doc[i].AsBsonDocument["quantity"].AsInt32 }).ToDictionary(i => i.key, i => i.value);
        }
    }
}

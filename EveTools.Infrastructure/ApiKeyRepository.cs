using System;
using System.Collections.Generic;
using System.Linq;
using EveAI.Live;
using EveTools.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace EveTools.Infrastructure
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private MongoCollection<ApiKey> _col;

        public ApiKeyRepository(MongoDatabase database)
        {
            _col = database.GetCollection<ApiKey>("apiKey");
        }

        public ICollection<ApiKey> GetAll()
        {
            var keys = _col.FindAll().ToList();
            keys.ForEach(UpdateKeyInfo);
            return keys;
        }

        public ApiKey GetById(string key)
        {
            var keys = _col.Find(new QueryDocument("_id", key)).ToList();
            keys.ForEach(UpdateKeyInfo);
            return keys.SingleOrDefault();
        }

        private void UpdateKeyInfo(ApiKey key)
        {
            if (null == key)
                return;
            if (key.keyInfo == null || key.keyInfoTimestamp == null ||
                key.keyInfoTimestamp.Value < DateTime.UtcNow.AddHours(-1))
            {
                // key info is missing or expired
                var info = new EveApi(key.keyId, key.vCode).getApiKeyInfo();
                key.keyInfo = info;
                key.keyInfoTimestamp = DateTime.UtcNow;
                var result = _col.Update(new QueryDocument("_id", key.id), Update.Replace(key));
                if(!result.UpdatedExisting || result.DocumentsAffected != 1)
                    throw new ApplicationException("Something broke :(");
            }
        }
    }
}

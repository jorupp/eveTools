using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveAI.Live.Account;
using MongoDB.Bson;

namespace EveTools.Domain
{
    public class ApiKey
    {
        public ObjectId id { get; set; }
        public long keyId { get; set; }
        public string vCode { get; set; }
        public string name { get; set; }
        public APIKeyInfo keyInfo { get; set; }
        public DateTime? keyInfoTimestamp { get; set; }
    }
}

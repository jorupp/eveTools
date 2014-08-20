using System.Collections.Generic;
using EveAI.Live;

namespace EveTools.Web.Models
{
    public class AssetSummary
    {
        public string Name { get; set; }
        public List<Asset> Assets { get; set; }
        public long? Quantity { get; set; }
        public long TotalVolume { get; set; }
        public decimal? UnitValue { get; set; }
        public decimal TotalValue { get; set; }
        public ICollection<AssetSummary> Children { get; set; } 
    }
}
using System.Collections.Generic;

namespace EveTools.Domain.Models
{
    public class BlueprintActivityMaterialInfo
    {
        public int ActivityId { get; set; }
        public Dictionary<int, int> Materials { get; set; }
        public Dictionary<int, int> Products { get; set; }
        public int Time { get; set; }
    }
}
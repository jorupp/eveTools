using System.Collections.Generic;

namespace EveTools.Domain.Models
{
    public class BlueprintMaterialInfo
    {
        public int TypeId { get; set; }
        public Dictionary<int, BlueprintActivityMaterialInfo> ActivityMaterialInfo { get; set; } 
    }
}
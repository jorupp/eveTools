using System.Collections.Generic;

namespace EveTools.Web.Models
{
    public class AssetsViewModel
    {
        public string Entity { get; set; }
        public List<AssetSummary> Assets { get; set; } 
    }
}
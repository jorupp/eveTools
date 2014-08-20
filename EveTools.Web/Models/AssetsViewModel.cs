using System.Collections.Generic;
using EveTools.Web.Controllers;

namespace EveTools.Web.Models
{
    public class AssetsViewModel
    {
        public string Entity { get; set; }
        public List<AssetSummary> Missing { get; set; } 
    }
}
using System.Collections.Generic;

namespace EveTools.Web.Models
{
    public class ProductionFromAssetsViewModel
    {
        public string Entity { get; set; }
        public List<MissingMaterialsLocationSummary> Missing { get; set; }
        public MissingMaterialsSummary Summary { get; set; }
    }
}
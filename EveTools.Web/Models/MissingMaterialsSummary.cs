namespace EveTools.Web.Models
{
    public class MissingMaterialsSummary
    {
        public MissingMaterialsLocationSummary.BlueprintInfo[] Blueprints { get; set; }
        public MaterialsInfo[] Materials { get; set; }

        public class MaterialsInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public long QuantityHave { get; set; }
            public long QuantityShouldHave { get; set; }
            public long QuantityNeed { get; set; }
            public long QuantityLeftover { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
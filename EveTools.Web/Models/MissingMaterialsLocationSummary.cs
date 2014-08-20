namespace EveTools.Web.Models
{
    public class MissingMaterialsLocationSummary
    {
        public string LocationName { get; set; }
        public BlueprintInfo[] Blueprints { get; set; }
        public MaterialsInfo[] Materials { get; set; }

        public class BlueprintInfo
        {
            public int BpId { get; set; }
            public string BpName { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int MaxProductionLimit { get; set; }
            public long Quantity { get; set; }
            public decimal ProducedValue { get; set; }
        }

        public class MaterialsInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public long QuantityHave { get; set; }
            public long QuantityShouldHave { get; set; }

            public long QuantityNeed
            {
                get
                {
                    if (QuantityShouldHave < QuantityHave)
                        return 0;
                    return QuantityShouldHave - QuantityHave;
                }
            }

            public decimal UnitPrice { get; set; }
        }
    }
}
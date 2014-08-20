namespace EveTools.Web.Models
{
    public class BlueprintInfo
    {
        public int ProductId { get; set; }
        public string Product { get; set; }
        public int Runs { get; set; }
        public ProductionRequirement MigratedInvented { get; set; }
        public ProductionRequirement NewInvented { get; set; }
        public ProductionRequirement LegacyInvented { get; set; }
        public decimal ProductValue { get; set; }
        public decimal ProductVolume { get; set; }
    }
}
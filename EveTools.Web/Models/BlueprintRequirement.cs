namespace EveTools.Web.Models
{
    public class BlueprintRequirement
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalVolume { get; set; }
    }
}
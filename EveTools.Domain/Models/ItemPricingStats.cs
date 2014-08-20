namespace EveTools.Domain.Models
{
    public class ItemPricingStats
    {
        public int typeId { get; set; }
        public ItemPricingStatsDetail buy { get; set; }
        public ItemPricingStatsDetail sell { get; set; }
        public ItemPricingStatsDetail all { get; set; }
    }
}
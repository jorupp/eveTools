namespace EveTools.Domain.Models
{
    public class ItemPricingStatsDetail
    {
        public long volume { get; set; }
        public decimal avg { get; set; }
        public decimal max { get; set; }
        public decimal min { get; set; }
        public decimal stddev { get; set; }
        public decimal median { get; set; }
        public decimal percentile { get; set; }
    }
}
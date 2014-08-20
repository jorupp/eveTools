namespace EveTools.Domain.Models
{
    public class ActivityMaterial
    {
        public int TypeId { get; set; }
        public int ActivityId { get; set; }
        public int MaterialTypeId { get; set; }
        public int Quantity { get; set; }
        public int Consume { get; set; }
    }
}
namespace EveTools.Domain.Models
{
    public class ActivityProduct
    {
        public int TypeId { get; set; }
        public byte ActivityId { get; set; }
        public int ProductTypeId { get; set; }
        public int Quantity { get; set; }
    }
}
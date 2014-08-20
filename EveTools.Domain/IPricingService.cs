using EveAI.Product;

namespace EveTools.Domain
{
    public interface IPricingService
    {
        decimal GetPrice(int typeId);
        decimal GetPrice(ProductType type);
    }
}
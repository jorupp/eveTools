using System.Collections.Generic;
using EveTools.Domain.Models;

namespace EveTools.Domain
{
    public interface IPricingRepository
    {
        ItemPricingStats[] GetStats(int? systemId, int? regionId);
    }
}
using EveTools.Domain.Models;

namespace EveTools.Domain
{
    public interface IStaticDataRepository
    {
        BlueprintMaterialInfo[] GetBlueprintMaterialInfo();
    }
}
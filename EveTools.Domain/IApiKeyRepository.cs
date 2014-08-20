using System.Collections.Generic;

namespace EveTools.Domain
{
    public interface IApiKeyRepository
    {
        ICollection<ApiKey> GetAll();
        ApiKey GetById(string key);
    }
}
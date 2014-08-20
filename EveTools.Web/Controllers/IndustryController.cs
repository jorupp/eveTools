using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI.Live;
using EveTools.Domain;

namespace EveTools.Web.Controllers
{
    public class IndustryController : Controller
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public IndustryController(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }

        // GET: Industry
        public ActionResult Index(string id, bool isCorp, int characterId)
        {
            var key = _apiKeyRepository.GetById(id);
            if (null == key)
                return HttpNotFound();
            var api = new EveApi(key.keyId, key.vCode, characterId);
            var jobs = isCorp ? api.GetCorporationIndustryJobs() : api.GetCharacterIndustryJobs();

            return Content(jobs.Count.ToString());
        }
    }
}
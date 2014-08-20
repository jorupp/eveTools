using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using EveAI.Live;
using EveTools.Domain;

namespace EveTools.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public HomeController(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }

        // GET: Home
        public ActionResult Index()
        {
            var keys = _apiKeyRepository.GetAll();

            return View(keys);
        }

        public ActionResult Assets(string id, bool isCorp, int characterId)
        {
            var key = _apiKeyRepository.GetById(id);
            if (null == key)
                return HttpNotFound();
            var api = new EveApi(key.keyId, key.vCode, characterId);
            var assets = isCorp ? api.GetCorporationAssets() : api.GetCharacterAssets();

            return View(assets);
        }
    }
}
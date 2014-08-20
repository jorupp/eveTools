using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using EveAI;
using EveAI.Live;
using EveTools.Domain;

namespace EveTools.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IPricingRepository _pricingRepository;

        public HomeController(IApiKeyRepository apiKeyRepository, IPricingRepository pricingRepository)
        {
            _apiKeyRepository = apiKeyRepository;
            _pricingRepository = pricingRepository;
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

        public ActionResult Prices()
        {
            var dataCore = new EveApi(true).EveApiCore;
            return Json(_pricingRepository.GetStats(dataCore.GetIdForObject(dataCore.SolarSystems.Single(i => i.Name == "Jita")), null), JsonRequestBehavior.AllowGet);
        }
    }
}
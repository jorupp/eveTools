using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using EveAI;
using EveAI.Live;
using EveTools.Domain;
using EveTools.Web.Models;

namespace EveTools.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IPricingService _pricingService;

        public HomeController(IApiKeyRepository apiKeyRepository, IPricingService pricingService)
        {
            _apiKeyRepository = apiKeyRepository;
            _pricingService = pricingService;
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

            var summary = BuildLocationSummaries(assets);
            summary.Sort((a,b) => a.TotalValue < b.TotalValue ? 1 : -1);

            return View(summary);
        }

        //public ActionResult Prices()
        //{
        //    var dataCore = new EveApi(true).EveApiCore;
        //    return Json(_pricingRepository.GetStats(dataCore.GetIdForObject(dataCore.SolarSystems.Single(i => i.Name == "Jita")), null), JsonRequestBehavior.AllowGet);
        //}

        private List<AssetSummary> BuildLocationSummaries(IEnumerable<Asset> assets)
        {
            var retVal = new List<AssetSummary>();
            foreach (var loc in assets.GroupBy(i => new { i.LocationID, i.LocationSolarsystem, i.LocationStation, i.LocationConquerableStation }).OrderBy(i => i.Key.LocationID))
            {
                foreach (var cc in loc.GroupBy(i => i.Container).OrderBy(i => i.Key))
                {
                    var locName = 
                        loc.Key.LocationConquerableStation != null ? loc.Key.LocationConquerableStation.StationName :
                        loc.Key.LocationStation != null ? loc.Key.LocationStation.Name :
                        loc.Key.LocationSolarsystem != null ? loc.Key.LocationSolarsystem.Name :
                        loc.Key.LocationID.ToString();

                    var children = BuildSummaries(cc);
                    retVal.Add(new AssetSummary()
                    {
                        Name = locName + " - " + cc.Key,
                        TotalVolume = children.Sum(i => i.TotalVolume),
                        TotalValue = children.Sum(i => i.TotalValue),
                        Children = children,
                    });
                }
            }
            return retVal;
        }

        private List<AssetSummary> BuildSummaries(IEnumerable<Asset> assets)
        {
            var retVal = new List<AssetSummary>();
            foreach (var c in assets.GroupBy(i => i.Type.Group.Category).OrderBy(i => i.Key.Name))
            {
                foreach (var g in c.GroupBy(i => i.Type.Group).OrderBy(i => i.Key.Name))
                {
                    foreach (var t in g.GroupBy(i => i.Type).OrderBy(i => i.Key.Name))
                    {
                        var unitValue = _pricingService.GetPrice(t.Key);
                        var collapse = t.Where(i => i.Contents == null).ToList();
                        var expand = t.Where(i => i.Contents != null);
                        if (collapse.Any())
                        {
                            var qty = collapse.Sum(i => i.Quantity);
                            retVal.Add(new AssetSummary()
                            {
                                Name = t.Key.Name,
                                Assets = collapse,
                                Quantity = qty,
                                TotalVolume = qty * (long)t.Key.Volume,
                                UnitValue = unitValue,
                                TotalValue = qty * unitValue,
                            });
                        }
                        foreach (var x in expand)
                        {
                            var children = BuildSummaries(x.Contents);
                            retVal.Add(new AssetSummary()
                            {
                                Name = t.Key.Name,
                                Assets = new List<Asset> { x },
                                Quantity = x.Quantity,
                                TotalVolume = (long)t.Key.Volume + children.Sum(i => i.TotalVolume),
                                UnitValue = unitValue,
                                TotalValue = unitValue + children.Sum(i => i.TotalValue),
                                Children = children,
                            });
                        }
                    }
                }
            }
            return retVal;
        } 

    }
}
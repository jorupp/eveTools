using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI;
using EveAI.Live;
using EveAI.Product;
using EveTools.Domain;
using EveTools.Domain.Models;
using EveTools.Web.Models;

namespace EveTools.Web.Controllers
{
    public class BlueprintsController : Controller
    {
        private readonly DataCore _dataCore;
        private readonly IPricingService _pricingService;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IApiKeyRepository _apiKeyRepository;

        public BlueprintsController(DataCore dataCore, IPricingService pricingService, IStaticDataRepository staticDataRepository, IApiKeyRepository apiKeyRepository)
        {
            _dataCore = dataCore;
            _pricingService = pricingService;
            _staticDataRepository = staticDataRepository;
            _apiKeyRepository = apiKeyRepository;
        }

        // GET: Blueprints
        public ActionResult Index()
        {
            var matInfo = _staticDataRepository.GetBlueprintMaterialInfo();
            var bps = _dataCore.ProductTypes
                .Where(i => i.CorrespondingBlueprint != null && i.CorrespondingBlueprint.Blueprint.TechLevel == 2)
                .Select(i =>
                {
                    var productId = _dataCore.GetIdForObject(i);
                    var bpId = _dataCore.GetIdForObject(i.CorrespondingBlueprint);
                    var maxProductionLimit = i.CorrespondingBlueprint.Blueprint.MaxProductionLimit;
                    return new BlueprintInfo()
                                 {
                                     ProductId = productId,
                                     Product = i.Name,
                                     Runs = maxProductionLimit,
                                     MigratedInvented = GetNewRequirements(matInfo, bpId, 7, maxProductionLimit),
                                     NewInvented = GetNewRequirements(matInfo, bpId, 4, maxProductionLimit),
                                     ProductValue = maxProductionLimit * _pricingService.GetPrice(i),
                                     ProductVolume = maxProductionLimit * (decimal)i.Volume,
                                 };
                }).ToList();
            return View(bps);
        }

        private ProductionRequirement GetNewRequirements(BlueprintMaterialInfo[] matInfo, int bpId, int meLevel, int numRuns)
        {
            var info = matInfo.Single(i => i.TypeId == bpId).ActivityMaterialInfo[(int)Activity.Manufacturing];
            var mats = info.Materials;

            var req = new ProductionRequirement()
            {
                JobDuration = TimeSpan.FromMinutes(numRuns * info.Time),
                Materials = mats.Select(i =>
                {
                    var type = _dataCore.FindProductType(i.Key);
                    var qty = Math.Max((int)Math.Ceiling((i.Value * numRuns) * (1 - .01m * meLevel)), numRuns);
                    return new BlueprintRequirement()
                                                 {
                                                     TypeId = i.Key,
                                                     TypeName = type.Name,
                                                     Quantity = qty,
                                                     TotalVolume = (decimal)type.Volume * qty,
                                                     TotalPrice = _pricingService.GetPrice(type) * qty,
                                                 };
                }).ToArray()
            };
            req.TotalCost = req.Materials.Sum(i => i.TotalPrice);
            return req;
        }

        public ActionResult ProductionFromT2Blueprints(string id, bool isCorp, int characterId)
        {
            var key = _apiKeyRepository.GetById(id);
            if (null == key)
                return HttpNotFound();
            var api = new EveApi(key.keyId, key.vCode, characterId);
            var assets = isCorp ? api.GetCorporationAssets() : api.GetCharacterAssets();

            var summary = BuildLocationSummaries(assets);
            var character = key.keyInfo.Characters.Single(i => i.CharacterID == characterId);
            var model = new ProductionFromAssetsViewModel()
            {
                Entity = isCorp ? character.CorporationName : character.Name,
                Missing = summary,
            };
            model.Summary = new MissingMaterialsSummary()
            {
                Blueprints = model.Missing.SelectMany(i => i.Blueprints).GroupBy(i => i.BpId).Select(i => new MissingMaterialsLocationSummary.BlueprintInfo()
                {
                    BpId = i.First().BpId,
                    BpName = i.First().BpName,
                    ProductId = i.First().ProductId,
                    ProductName = i.First().ProductName,
                    MaxProductionLimit = i.First().MaxProductionLimit,
                    TimeForMaxRun = i.First().TimeForMaxRun,
                    Quantity = i.Sum(ii => ii.Quantity),
                    ProducedValue = i.Sum(ii => ii.ProducedValue),
                }).ToArray(),
                Materials = model.Missing.SelectMany(i => i.Materials).GroupBy(i => i.Id).Select(i => new MissingMaterialsSummary.MaterialsInfo
                {
                    Id = i.First().Id,
                    Name = i.First().Name,
                    QuantityShouldHave = i.Sum(ii => ii.QuantityShouldHave),
                    QuantityHave = i.Sum(ii => ii.QuantityHave),
                    QuantityNeed = i.Sum(ii => ii.QuantityNeed),
                    QuantityLeftover = i.Sum(ii => ii.QuantityHave - ii.QuantityShouldHave + ii.QuantityNeed),
                    UnitPrice = i.First().UnitPrice,
                }).ToArray()
            };
            return View(model);
        }


        private List<MissingMaterialsLocationSummary> BuildLocationSummaries(IEnumerable<Asset> assets)
        {
            var matInfo = _staticDataRepository.GetBlueprintMaterialInfo();
            var retVal = new List<MissingMaterialsLocationSummary>();
            foreach (var loc in assets.GroupBy(i => new { i.LocationID, i.LocationSolarsystem, i.LocationStation, i.LocationConquerableStation }).OrderBy(i => i.Key.LocationID))
            {
                var locName =
                    loc.Key.LocationConquerableStation != null ? loc.Key.LocationConquerableStation.StationName :
                    loc.Key.LocationStation != null ? loc.Key.LocationStation.Name :
                    loc.Key.LocationSolarsystem != null ? loc.Key.LocationSolarsystem.Name :
                    loc.Key.LocationID.ToString();

                var deepAssets = loc.Recursive(i => (IEnumerable<Asset>)i.Contents ?? new Asset[0]).ToList();

                var bps = deepAssets.Where(i => i.Type.Blueprint != null && i.Type.Blueprint.TechLevel == 2).GroupBy(i => i.Type).ToList();
                if (bps.Count == 0)
                    continue;

                var moduleDefaultMaxRun = 10; //  should be 1 for ships, but we don't have any, so that's ok
                var s = new MissingMaterialsLocationSummary()
                {
                    LocationName = locName,
                    Blueprints = (from i in bps
                                    let bpId = _dataCore.GetIdForObject(i.Key)
                                    join mi in matInfo on bpId equals mi.TypeId into mig
                                    from mi in mig.DefaultIfEmpty()
                                  select new MissingMaterialsLocationSummary.BlueprintInfo()
                                    {
                                        BpId = bpId,
                                        BpName = i.Key.Name,
                                        ProductId = _dataCore.GetIdForObject(i.Key.Blueprint.Product),
                                        ProductName = i.Key.Blueprint.Product.Name,
                                        MaxProductionLimit = i.Key.Blueprint.MaxProductionLimit,
                                        Quantity = i.Sum(ii => ii.Quantity),
                                        ProducedValue = i.Sum(ii => ii.Quantity) * _pricingService.GetPrice(i.Key.Blueprint.Product) * moduleDefaultMaxRun,
                                        TimeForMaxRun = TimeSpan.FromSeconds(moduleDefaultMaxRun * mi.ActivityMaterialInfo[(int)Activity.Manufacturing].Time * 0.585 /** derived from actual **/),
                                    }).ToArray(),
                };
                retVal.Add(s);

                s.Materials =
                    (from bp in s.Blueprints
                        from r in GetNewRequirements(matInfo, bp.BpId, 7, moduleDefaultMaxRun).Materials
                        let itemsRequired = bp.Quantity*r.Quantity
                        group itemsRequired by new {r.TypeId, r.TypeName}
                        into g
                        select new MissingMaterialsLocationSummary.MaterialsInfo()
                        {
                            Id = g.Key.TypeId,
                            Name = g.Key.TypeName,
                            QuantityShouldHave = g.Sum(ii => ii),
                            UnitPrice = _pricingService.GetPrice(g.Key.TypeId),
                        }).ToArray();

                var qty = deepAssets.GroupBy(i => i.TypeID).ToDictionary(i => i.Key, i => i.Sum(ii => ii.Quantity));
                foreach (var mat in s.Materials)
                {
                    long matQty;
                    if (!qty.TryGetValue(mat.Id, out matQty))
                        continue;
                    mat.QuantityHave += matQty;
                }
            }
            return retVal;
        }

    }
}
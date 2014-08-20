using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI;
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

        public BlueprintsController(DataCore dataCore, IPricingService pricingService, IStaticDataRepository staticDataRepository)
        {
            _dataCore = dataCore;
            _pricingService = pricingService;
            _staticDataRepository = staticDataRepository;
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
                    var qty = Math.Max((int)Math.Ceiling((i.Value * numRuns) * (1-.01m*meLevel)), numRuns);
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
    }
}
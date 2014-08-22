﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI.Live;
using EveTools.Domain;
using EveTools.Web.Models;

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
            var currentJobs = isCorp ? api.Get<CorpUpdatedIndustryJobApi>().Data : api.Get<CharUpdatedIndustryJobApi>().Data; ;
            var allJobs = isCorp ? api.Get<CorpHistoryUpdatedIndustryJobApi>().Data : api.Get<CharHistoryUpdatedIndustryJobApi>().Data;

            var model = (from j in allJobs
                         group j.Start by new { j.InstallerID, j.Activity }
                             into groups
                             join current in currentJobs.GroupBy(i => new { i.InstallerID, i.Activity }) on groups.Key equals
                                 current.Key into currentGroup
                             from current in currentGroup.DefaultIfEmpty()
                             let timeRemaining = (current ?? (IEnumerable<UpdatedIndustryJob>)new UpdatedIndustryJob[0]).Select(i => i.End.Subtract(DateTime.UtcNow)).Where(i => i.TotalSeconds > 0).ToList()
                             select new IndustryStatusModel
                             {
                                 InstallerID = groups.Key.InstallerID,
                                 InstallerName = api.GetCharacterNameLookup(new List<long>() { groups.Key.InstallerID })[groups.Key.InstallerID],
                                 Activity = groups.Key.Activity,
                                 LatestInstalledJob = groups.Max(),
                                 CurrentJobCount = timeRemaining.Count(),
                                 TimeUntilNextComplete = timeRemaining.Cast<TimeSpan?>().FirstOrDefault()
                             }).ToList();

            return View(model);
        }
    }
}
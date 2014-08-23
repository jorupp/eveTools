using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI.Live;
using EveAI.Live.Account;
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
            var accountMap =
                _apiKeyRepository.GetAll()
                    .SelectMany(i => i.keyInfo.Characters.Select(ii => new {ii.CharacterID, AccountName = i.name}))
                    .GroupBy(i => i.CharacterID)
                    .ToDictionary(i => i.Key, i => i.First().AccountName);
            var api = new EveApi(key.keyId, key.vCode, characterId);
            var currentJobs = isCorp ? api.Get<CorpUpdatedIndustryJobApi>().Data : api.Get<CharUpdatedIndustryJobApi>().Data; ;
            var allJobs = isCorp ? api.Get<CorpHistoryUpdatedIndustryJobApi>().Data : api.Get<CharHistoryUpdatedIndustryJobApi>().Data;

            var model = (from j in allJobs
                         group j by new { j.InstallerID, j.Activity } into groups
                             join current in currentJobs.GroupBy(i => new { i.InstallerID, i.Activity }) on groups.Key equals
                                 current.Key into currentGroup
                             from current in currentGroup.DefaultIfEmpty()
                             let timeRemaining = (current ?? (IEnumerable<UpdatedIndustryJob>)new UpdatedIndustryJob[0]).Select(i => i.End.Subtract(DateTime.UtcNow)).Where(i => i.TotalSeconds > 0).OrderBy(i => i.TotalSeconds).ToList()
                             let maxConcurrent = GetMaxConcurrent(groups)
                             select new IndustryStatusModel
                             {
                                 AccountName = accountMap.FirstOrDefault(i => i.Key == groups.Key.InstallerID).Value,
                                 InstallerID = groups.Key.InstallerID,
                                 InstallerName = api.GetCharacterNameLookup(new List<long>() { groups.Key.InstallerID })[groups.Key.InstallerID],
                                 Activity = groups.Key.Activity,
                                 LatestInstalledJob = groups.Max(i => i.Start),
                                 MaxJobCount = maxConcurrent,
                                 CurrentJobCount = timeRemaining.Count(),
                                 TimeUntilNextComplete = timeRemaining.Cast<TimeSpan?>().FirstOrDefault(),
                                 TimeUntilLastComplete = timeRemaining.Cast<TimeSpan?>().LastOrDefault(),
                             }).ToList();

            return View(model);
        }

        private int GetMaxConcurrent(IEnumerable<UpdatedIndustryJob> jobs)
        {
            return 
                (from j1 in jobs
                 from j2 in jobs
                 where (j1.Start <= j2.Start && j2.Start <= j1.End) || (j1.Start <= j2.End && j2.End <= j1.End)
                 group j2 by j1.JobID into g
                 select g.Count()).Where(i => i <= 11).Max();
        }
    }
}
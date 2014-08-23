using System;
using EveAI.Product;

namespace EveTools.Web.Models
{
    public class IndustryStatusModel
    {
        public string AccountName { get; set; }
        public long InstallerID { get; set; }
        public string InstallerName { get; set; }
        public Activity Activity { get; set; }
        public DateTime LatestInstalledJob { get; set; }
        public int MaxJobCount { get; set; }
        public int CurrentJobCount { get; set; }
        public TimeSpan? TimeUntilNextComplete { get; set; }
        public TimeSpan? TimeUntilLastComplete { get; set; }
    }
}
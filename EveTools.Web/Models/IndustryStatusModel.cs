using System;
using EveAI.Product;

namespace EveTools.Web.Models
{
    public class IndustryStatusModel
    {
        public long InstallerID { get; set; }
        public string InstallerName { get; set; }
        public Activity Activity { get; set; }
        public DateTime LatestInstalledJob { get; set; }
        public int CurrentJobCount { get; set; }
        public TimeSpan? TimeUntilNextComplete { get; set; }
    }
}
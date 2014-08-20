using System;

namespace EveTools.Web.Models
{
    public class ProductionRequirement
    {
        public BlueprintRequirement[] Materials { get; set; }
        public TimeSpan JobDuration { get; set; }
        public decimal JobCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
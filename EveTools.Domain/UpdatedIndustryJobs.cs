using System;
using System.Collections.Generic;
using System.Xml;
using EveAI.Live;
using EveAI.Live.Utility;
using EveAI.Product;

namespace EveTools.Domain
{
    public abstract class UpdatedIndustryJobApi : IndustryJobApi
    {
        private List<UpdatedIndustryJob> data = new List<UpdatedIndustryJob>();

        public new List<UpdatedIndustryJob> Data
        {
            get
            {
                return this.data;
            }
        }

        protected override void ProcessXmlData(XmlNode resultNode, bool clearExistingData)
        {
            if (clearExistingData)
                this.data = new List<UpdatedIndustryJob>();
            foreach (XmlNode node in resultNode["rowset"].ChildNodes)
                this.data.Add(new UpdatedIndustryJob()
                {
                    InstalledItem =
                    {
                        TypeID = EveApiBase.ReadXmlInt32(node, "installedItemTypeID"),
                        ItemID = EveApiBase.ReadXmlInt64(node, "installedItemID"),
                        LocationID = EveApiBase.ReadXmlInt64(node, "installedItemLocationID"),
                        Quantity = EveApiBase.ReadXmlInt64(node, "installedItemQuantity"),
                        ProductivityLevel = EveApiBase.ReadXmlInt32(node, "installedItemProductivityLevel"),
                        MaterialLevel = EveApiBase.ReadXmlInt32(node, "installedItemMaterialLevel"),
                        LicensedProductionRunsRemaining = EveApiBase.ReadXmlInt32(node, "installedItemLicensedProductionRunsRemaining"),
                        IsItemCopyInstalled = EveApiBase.ReadXmlBool(node, "installedItemCopy"),
                        Flag = EveApiBase.ReadXmlInt32(node, "installedItemFlag")
                    },
                    Output =
                    {
                        LocationID = EveApiBase.ReadXmlInt64(node, "outputLocationID"),
                        ItemTypeID = EveApiBase.ReadXmlInt32(node, "outputTypeID"),
                        Runs = EveApiBase.ReadXmlInt32(node, "runs"),
                        LicensedProductionRuns = EveApiBase.ReadXmlInt32(node, "licensedProductionRuns"),
                        Flag = EveApiBase.ReadXmlInt32(node, "outputFlag")
                    },
                    InstallLocation =
                    {
                        AssemblyLineID = EveApiBase.ReadXmlInt32(node, "assemblyLineID"),
                        ContainerID = EveApiBase.ReadXmlInt64(node, "containerID"),
                        InstalledInSolarSystemID = EveApiBase.ReadXmlInt32(node, "installedInSolarSystemID"),
                        ContainerLocationID = EveApiBase.ReadXmlInt64(node, "containerLocationID"),
                        MaterialMultiplier = EveApiBase.ReadXmlDouble(node, "materialMultiplier"),
                        TimeMultiplier = EveApiBase.ReadXmlDouble(node, "timeMultiplier"),
                        ContainerTypeID = EveApiBase.ReadXmlInt32(node, "containerTypeID")
                    },
                    JobID = EveApiBase.ReadXmlInt64(node, "jobID"),
                    InstallerID = EveApiBase.ReadXmlInt64(node, "installerID"),
                    CharacterMaterialMultiplier = EveApiBase.ReadXmlDouble(node, "charMaterialMultiplier"),
                    CharacterTimeMultiplier = EveApiBase.ReadXmlDouble(node, "charTimeMultiplier"),
                    IsCompleted = EveApiBase.ReadXmlBool(node, "completed"),
                    IsCompletedSuccessfully = EveApiBase.ReadXmlBool(node, "completedSuccessfully"),
                    Activity = (Activity)EveApiBase.ReadXmlInt32(node, "activityID"),
                    CompletedStatus = (IndustryJob.CompletedStatusType)EveApiBase.ReadXmlInt32(node, "completedStatus"),
                    InstallTime = EveApiBase.ReadXmlDateTime(node, "installTime"),
                    ProductionBegin = EveApiBase.ReadXmlDateTime(node, "beginProductionTime"),
                    ProductionEnd = EveApiBase.ReadXmlDateTime(node, "endProductionTime"),
                    ProductionPause = EveApiBase.ReadXmlDateTime(node, "pauseProductionTime"),
                    Start = EveApiBase.ReadXmlDateTime(node, "startDate"),
                    End = EveApiBase.ReadXmlDateTime(node, "endDate"),
                });
        }
    }

    public class UpdatedIndustryJob : IndustryJob
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class CorpUpdatedIndustryJobApi : UpdatedIndustryJobApi
    {
        protected override string RelativePath
        {
            get { return "/corp/IndustryJobs.xml.aspx"; }
        }
    }

    public class CorpHistoryUpdatedIndustryJobApi : UpdatedIndustryJobApi
    {
        protected override string RelativePath
        {
            get { return "/corp/IndustryJobsHistory.xml.aspx"; }
        }
    }

    public class CharUpdatedIndustryJobApi : UpdatedIndustryJobApi
    {
        protected override string RelativePath
        {
            get { return "/char/IndustryJobs.xml.aspx"; }
        }
    }

    public class CharHistoryUpdatedIndustryJobApi : UpdatedIndustryJobApi
    {
        protected override string RelativePath
        {
            get { return "/char/IndustryJobsHistory.xml.aspx"; }
        }
    }
}

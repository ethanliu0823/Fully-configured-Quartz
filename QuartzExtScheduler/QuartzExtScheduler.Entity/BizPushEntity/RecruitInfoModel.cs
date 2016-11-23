using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.BizPushEntity
{
    public class RecruitInfoModel
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }
        public int IsRegisted { get; set; }
        public string RegisterFund { get; set; }
        public string ServiceQuals { get; set; }
        public string AreaCode { get; set; }
        public string Area { get; set; }
        public string CaseNum { get; set; }
        public string OthersCondition { get; set; }
        public string DeveloperType { get; set; }
        public string RegisterCondition { get; set; }
    }
}

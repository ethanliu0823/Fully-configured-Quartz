using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.BizPushEntity
{
    public class BiddingInfoModel
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }
        public string DeveloperType { get; set; }
        public string IsRegisted { get; set; }
        public string Area { get; set; }
    }
}

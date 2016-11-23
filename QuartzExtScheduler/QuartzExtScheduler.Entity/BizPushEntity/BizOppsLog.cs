using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.BizPushEntity
{
    public class BizOppsLog
    {
        public string Uid { get; set; }
        public string CleanId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Mobile { get; set; }
        public string DeveloperName { get; set; }
        public string Chanel { get; set; }
        public string Type { get; set; }
        public string TempateId { get; set; }
        public DateTime CleanCreatedTime { get; set; }
        public string CleanCreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
    }
}

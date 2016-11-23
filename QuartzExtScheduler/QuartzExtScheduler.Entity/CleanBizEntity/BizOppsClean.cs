using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CleanBizEntity
{
    public class BizOppsClean
    {
        public string UID { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string OpenId { get; set; }

        public string SupplierId { get; set; }

        public string Type { get; set; }

        public bool Signed { get; set; }

        public DateTime? CreatedTime { get; set; }

        public string Created_By { get; set; }
    }
}

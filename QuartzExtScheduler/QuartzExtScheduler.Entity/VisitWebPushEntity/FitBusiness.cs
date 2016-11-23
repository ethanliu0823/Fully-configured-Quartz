using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.VisitWebPushEntity
{
    public class FitBusiness
    {
        /// <summary>
        /// 开发商名称
        /// </summary>
        public string DeveloperName { get; set; }
        /// <summary>
        /// 招募招标名称
        /// </summary>
        public string BusinessName { get; set; }
        /// <summary>
        /// 招募招标区域
        /// </summary>
        public string BusinessArea { get; set; }
        /// <summary>
        /// 招募招标ID
        /// </summary>
        public string Item_ID { get; set; }
        /// <summary>
        /// 同行报名数
        /// </summary>
        public Int64 Peer_Reg_num { get; set; }
        /// <summary>
        /// 招募r/招标b
        /// </summary>
        public string Type { get; set; }

    }
}

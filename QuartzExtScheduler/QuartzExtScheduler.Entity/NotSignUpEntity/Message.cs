using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.NotSignUpEntity
{
    /// <summary>
    /// 待发消息实体
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 供应商Id
        /// </summary>
        public string SupplierId { get; set; }

        /// <summary>
        /// 报名次数
        /// </summary>
        public int RegisterCount { get; set; }

        /// <summary>
        /// 入库次数
        /// </summary>
        public int StorageCount { get; set; }

        /// <summary>
        /// 入库家数
        /// </summary>
        public int StorageNum { get; set; }

        /// <summary>
        /// 案例数
        /// </summary>
        public int CaseNum { get; set; }

        /// <summary>
        /// 符合商机数
        /// </summary>
        public int FitNum { get; set; }

        /// <summary>
        /// 最近报名时间
        /// </summary>
        public DateTime? LastRegisterDate { get; set; }

        /// <summary>
        /// 开发商名称
        /// </summary>
        public string DeveloperName { get; set; }

        /// <summary>
        /// 开发商Id
        /// </summary>
        public string DeveloperId { get; set; }

        /// <summary>
        /// 运营顾问
        /// </summary>
        public string Counselor { get; set; }

        /// <summary>
        /// 运营顾问微信
        /// </summary>
        public string CounselorWxCode { get; set; }

        /// <summary>
        /// 同行名称
        /// </summary>
        public string ColleagueName { get; set; }

        /// <summary>
        /// 同行Id
        /// </summary>
        public string ColleagueId { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }


        /// <summary>
        /// 分类代码
        /// </summary>
        public string CategoryCode { get; set; }
    }
}

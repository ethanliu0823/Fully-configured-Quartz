using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.VisitWebPushEntity
{
    /// <summary>
    /// 发送用户信息
    /// </summary>
    public class UserModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string OpenId { get; set; }
        public string CompanyId { get; set; }
        public string ConsultantName { get; set; }
        public string ConsultantWechat { get; set; }
    }
}

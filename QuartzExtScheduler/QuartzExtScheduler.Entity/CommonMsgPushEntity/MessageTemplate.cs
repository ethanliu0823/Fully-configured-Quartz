using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonMsgPushEntity
{
    public class MessageTemplate
    {
        public string MessageType { get; set; }
        public string Template_ID { get; set; }
        public string Only_One { get; set; }
        public string Wechat_Template_Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity
{
    public enum SendType
    {
        /// <summary>
        /// 邮件
        /// </summary>
        EmailQueue,
        /// <summary>
        /// 短信
        /// </summary>
        SmsQueue,
        /// <summary>
        /// 微信模板消息
        /// </summary>
        WxQueue,
        /// <summary>
        /// 微信客服文本消息
        /// </summary>
        WxCustomQueue,
        /// <summary>
        /// 微信客服图文消息
        /// </summary>
        WxCustomNewsQueue
    }
}

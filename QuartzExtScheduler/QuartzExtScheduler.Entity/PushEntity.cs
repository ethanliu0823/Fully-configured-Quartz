using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzExtScheduler.Entity
{
    public class PushEntity
    {
        /// <summary>
        /// 需求单ID，主动定向营销中使用
        /// </summary>
        public string Business_ID { get; set; }
        /// <summary>
        /// 业务类型 【数据源：op_push_msg_business_type】
        /// </summary>
        public string Business_Type { get; set; }
        /// <summary>
        /// 微信发送控制类型 1: 一周一批（一批最多最多五条） 2：一次/1人天
        /// </summary>
        public string Send_Control_Type { get; set; }
        /// <summary>
        /// 短信发送控制类型
        /// </summary>
        public string SMS_Send_Control_Type { get; set; }
        /// <summary>
        /// 邮件发送控制类型
        /// </summary>
        public string Email_Send_Control_Type { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// open_id
        /// </summary>
        public string OpenID { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 发送类型 EmailQueue,SmsQueue,WechatQueue
        /// </summary>
        public SendType SendType { get; set; }
        /// <summary>
        /// 发送标题
        /// </summary>
        public string SendTitle { get; set; }
        /// <summary>
        /// 发送内容
        /// </summary>
        public string SendContent { get; set; }
        /// <summary>
        /// 用来标识消费端对此消息处理的方式(比如邮件系统是调用模版发送接口还是普通邮件发送接口)
        /// </summary>
        public string MessageType { get; set; }
        /// <summary>
        /// 消息发送者
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 推送备注：描述
        /// </summary>
        public string remark { get; set; }
    }
}

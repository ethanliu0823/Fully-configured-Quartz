using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QuartzExtScheduler.Utility.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using QuartzExtScheduler.Common;
using QuartzExtScheduler.Entity;
using Common.Logging;
using System.Text.RegularExpressions;

namespace QuartzExtScheduler.DataAccess
{
    /// <summary>
    /// 消息队列
    /// </summary>
    public class MessageQueue
    {
        private static ILog _log = LogManager.GetCurrentClassLogger();
        public const string MessageQueueKey = "b2b_support.bizp_message_queue";

        public const string MessageBatchKey = "b2b_support.queue.messageBatch";
        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="queueId"></param>
        /// <param name="messageTypeCode"></param>
        /// <param name="title"></param>
        /// <param name="messageBatch"></param>
        /// <param name="sender"></param>
        /// <param name="senderId"></param>
        /// <param name="channel"></param>
        /// <param name="receiver"></param>
        /// <param name="content"></param>
        /// <param name="attachments"></param>
        /// <param name="remark"></param>
        /// <param name="status"></param>
        /// <param name="tryTimes"></param>
        /// <returns></returns>
        private static bool SendMessageQueue(string connectionString, string queueId, string messageTypeCode, string title,
            string messageBatch, string sender, string senderId, string channel, string receiver, string content,
            string attachments, string remark, int status, int tryTimes)
        {
            //微信被封后封闭入口，梳理好后再开放
            //if (channel == "4") return false;

            var dicReceiver = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(receiver.Trim('[').Trim(']'));
            Dictionary<string, string> dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(dicReceiver["addrMap"]));
            string contact = dic.Values.First();

            if (channel == "4")//微信
            {
                if (IsExistsWechatBlackList(connectionString, contact))
                {
                    return false;
                }
            }
            else if (channel == "2")//email
            {
                if (IsExistsMailBlackList(connectionString, contact))
                {
                    return false;
                }
            }
            else if (channel == "7")//sms
            {
                if (IsExistsSMSBlackList(connectionString, contact))
                {
                    return false;
                }
            }
            else
            {

            }
            string saveSQL = @"INSERT INTO b2b_support.`bizp_message_queue`(
	                            `queue_id`,
	                            `message_type_code`,
	                            `title`,
	                            `message_batch`,
	                            `sender`,`sender_id`,`channel`,
	                            `receiver`,
	                            `content`,
	                            `attachments`,
	                            `remark`,
	                            `status`,
	                            `try_times`
                            )
                            VALUES(@queueId,@messageTypeCode,@title,@messageBatch,@sender,@senderId,@channel,@receiver,@content,@attachments,@remark,@status,@tryTimes)";
            var saveParames = new[]
            {
                new MySqlParameter("@queueId", queueId),
                new MySqlParameter("@messageTypeCode", messageTypeCode),
                new MySqlParameter("@title", title),
                new MySqlParameter("@messageBatch", messageBatch),
                new MySqlParameter("@sender", sender),
                new MySqlParameter("@senderId", senderId),
                new MySqlParameter("@channel", channel),
                new MySqlParameter("@receiver", receiver),
                new MySqlParameter("@content", content),
                new MySqlParameter("@attachments", attachments),
                new MySqlParameter("@remark", remark),
                new MySqlParameter("@status", Convert.ToString(status)),
                new MySqlParameter("@tryTimes", Convert.ToString(tryTimes))
            };
            return MySQLHelper.ExecuteNonQuery(connectionString, saveSQL, saveParames) > 0;
        }

        /// <summary>
        /// 发送消息队列--with monitor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="queueId"></param>
        /// <param name="messageTypeCode"></param>
        /// <param name="title"></param>
        /// <param name="messageBatch"></param>
        /// <param name="sender"></param>
        /// <param name="senderId"></param>
        /// <param name="channel"></param>
        /// <param name="receiver"></param>
        /// <param name="content"></param>
        /// <param name="attachments"></param>
        /// <param name="remark"></param>
        /// <param name="status"></param>
        /// <param name="tryTimes"></param>
        /// <param name="monitor_batch"></param>
        /// <param name="userId"></param>
        /// <param name="wayInfo"></param>
        /// <returns></returns>
        private static bool SendMessageQueue(string connectionString, string queueId, string messageTypeCode, string title,
    string messageBatch, string sender, string senderId, string channel, string receiver, string content,
    string attachments, string remark, int status, int tryTimes, string monitor_batch, string userId, string wayInfo ,string businessType)
        {
            //微信被封后封闭入口，梳理好后再开放
            //if (channel == "4") return false;
            int successful_count = 0;
            var dicReceiver = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(receiver.Trim('[').Trim(']'));
            Dictionary<string, string> dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(dicReceiver["addrMap"]));
            string contact = dic.Values.First();

            if (channel == "4")//微信
            {
                if (IsExistsWechatBlackList(connectionString, contact) || IsBizPushBlack(connectionString, userId))
                {
                    return false;
                }
            }
            else if (channel == "2")//email
            {
                if (IsExistsMailBlackList(connectionString, contact))
                {
                    return false;
                }
            }
            else if (channel == "7")//sms
            {
                if (IsExistsSMSBlackList(connectionString, contact) || IsBizPushBlack(connectionString, userId, 2))
                {
                    return false;
                }
            }
            else
            {

            }
            string saveSQL = @"INSERT INTO b2b_support.`bizp_message_queue`(
	                            `queue_id`,
	                            `message_type_code`,
	                            `title`,
	                            `message_batch`,
	                            `sender`,`sender_id`,`channel`,
	                            `receiver`,
	                            `content`,
	                            `attachments`,
	                            `remark`,
	                            `status`,
	                            `try_times`
                            )
                            VALUES(@queueId,@messageTypeCode,@title,@messageBatch,@sender,@senderId,@channel,@receiver,@content,@attachments,@remark,@status,@tryTimes)";
            var saveParames = new[]
            {
                new MySqlParameter("@queueId", queueId),
                new MySqlParameter("@messageTypeCode", messageTypeCode),
                new MySqlParameter("@title", title),
                new MySqlParameter("@messageBatch", messageBatch),
                new MySqlParameter("@sender", sender),
                new MySqlParameter("@senderId", senderId),
                new MySqlParameter("@channel", channel),
                new MySqlParameter("@receiver", receiver),
                new MySqlParameter("@content", content),
                new MySqlParameter("@attachments", attachments),
                new MySqlParameter("@remark", remark),
                new MySqlParameter("@status", Convert.ToString(status)),
                new MySqlParameter("@tryTimes", Convert.ToString(tryTimes))
            };
            bool result = MySQLHelper.ExecuteNonQuery(connectionString, saveSQL, saveParames) > 0;

            #region 插入监控表
            DataTable dt = BatchNoIsExisted(connectionString, monitor_batch);
            bool isSameBatch = dt.Rows.Count > 0;
            string activityID;
            //同批次activityID使用原来的
            if (isSameBatch)
            {
                activityID = dt.Rows[0]["uid"].ToString();
            }
            else
            {
                activityID = Guid.NewGuid().ToString();
            }

            PushEntity entity = new PushEntity
            {
                UserID=userId,
                Mobile=channel=="7"?wayInfo:null,
                OpenID=channel=="4"?wayInfo:null,
                Email=channel=="2"?wayInfo:null,
                SendContent = content,
                Send_Control_Type = channel == "7"? SendType.SmsQueue.ToString():channel=="4"?SendType.WxQueue.ToString():channel=="2"?SendType.EmailQueue.ToString():string.Empty
            };

            bool s2 = InsertPushMessageDetail(connectionString, entity, activityID);

            if (result && s2)
            {
                successful_count++;
            }

            #endregion

            //批次号更新逻辑：存在更新成功次数，不存在插入
            if (!isSameBatch)
            {
                InsertPushMessageActivity(connectionString, activityID, entity.UserID, businessType, monitor_batch, successful_count, remark);
            }
            else
            {
                UpdatePushMessageActivity(connectionString, monitor_batch, successful_count);
            }

            return result;
        }

        /// <summary>
        /// 是否存在退订黑名单中
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsBizPushBlack(string connectionString,string user_id, int type = 1)
        {
            string sql = @"select count(1) from dotnet_operation.op_message_push_blacklist where user_id=@user_id and type=@type";

            int count = (int)(MySQLHelper.ExecuteScalar(connectionString,sql, new MySqlParameter("user_id", user_id), new MySqlParameter("type", type)));

            return count > 0;
        }

        /// <summary>
        /// 查看监控主表中是否存在batchID
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="batchID"></param>
        /// <returns></returns>
        public static DataTable BatchNoIsExisted(string connectionString, string batchID)
        {
            string sql = "select uid From `dotnet_operation`.`op_info_push_activity` where message_batch=@message_batch";
            DataTable dt = MySQLHelper.ExecuteDataTable(connectionString,sql, new MySqlParameter("@message_batch", batchID));
            return dt;
        }

        /// <summary>
        /// 插入推送明细表
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entity"></param>
        /// <param name="batchID"></param>
        /// <param name="activityID"></param>
        /// <returns></returns>
        public static bool InsertPushMessageDetail(string connectionString, PushEntity entity, string activityID)
        {
            string sql = @"INSERT INTO `dotnet_operation`.`op_info_push_detail` (`uid`, `activity_id`, `user_id`, `mobile`, `open_id`, `mail`, `push_content`, `push_type`) 
VALUES (UUID(), @activity_id, @user_id, @mobile, @open_id, @mail, @push_content, @push_type)";
            return MySQLHelper.ExecuteNonQuery(connectionString,sql, new[] 
                                                 {
                                                     new MySqlParameter("@activity_id",activityID),
                                                     new MySqlParameter("@user_id",entity.UserID),
                                                     new MySqlParameter("@mobile",entity.Mobile),
                                                     new MySqlParameter("@open_id",entity.OpenID),
                                                     new MySqlParameter("@mail",entity.Email),
                                                     new MySqlParameter("@push_content",entity.SendContent),
                                                     new MySqlParameter("@push_type",entity.SendType.ToString())
                                                 }) > 0;

        }


        /// <summary>
        /// 插入信息推送活动表
        /// </summary>
        /// <param name="db"></param>
        /// <param name="activityID"></param>
        /// <param name="businessID"></param>
        /// <param name="businessType"></param>
        /// <param name="batchID"></param>
        /// <param name="pushNum"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool InsertPushMessageActivity(string connectionString, string activityID, string businessID, string businessType, string batchID, int pushNum, string remark)
        {
            string sql = @"INSERT INTO `dotnet_operation`.`op_info_push_activity` (`uid`, `item_id`, `item_type`, `message_batch`, `push_num`, `remark`, `created_by`)
 VALUES (@uid, @item_id, @item_type, @message_batch, @push_num, @remark, @created_by)";
            return MySQLHelper.ExecuteNonQuery(connectionString,sql, new[] 
                                                 {
                                                     new MySqlParameter("@uid",activityID),
                                                     new MySqlParameter("@item_id",businessID),
                                                     new MySqlParameter("@item_type",businessType),
                                                     new MySqlParameter("@message_batch",batchID),
                                                     new MySqlParameter("@push_num",pushNum),
                                                     new MySqlParameter("@remark",remark),
                                                     new MySqlParameter("@created_by",1)
                                                 }) > 0;
        }

        public static bool UpdatePushMessageActivity(string connectionString, string batchID, int pushNum)
        {
            string sql = @"update `dotnet_operation`.`op_info_push_activity` set push_num=push_num+@push_num where message_batch=@message_batch";
            return MySQLHelper.ExecuteNonQuery(connectionString, sql, new[]
                                            {
                                                new MySqlParameter("@push_num",pushNum),
                                                new MySqlParameter("@message_batch",batchID)
                                            }) > 0;
        }

        /// <summary>
        /// 是否存在于微信黑名单
        /// </summary>
        /// <returns></returns>
        private static bool IsExistsWechatBlackList(string connectionString, string receiver)
        {
            string sql = @"select mr.userid from b2b_support.bizp_message_wx_blacklist blk
                inner join m_yct.m_wx2ycguser mr on blk.openId=mr.openid where is_valid=1 and ifnull(mr.userid,'') <>'' and mr.userid=@user_id;";
            object result = MySQLHelper.ExecuteScalar(connectionString, sql, new MySqlParameter("@user_id", receiver));
            return result != DBNull.Value && result != null;
        }

        /// <summary>
        /// 是否存在于手机黑名单
        /// </summary>
        /// <returns></returns>
        private static bool IsExistsSMSBlackList(string connectionString, string receiver)
        {
            var sql = @"select mobile from b2b_support.bizp_message_sms_blacklist where is_valid=1 and ifnull(mobile,'')<>'' and mobile=@mobile;";
            object result = MySQLHelper.ExecuteScalar(connectionString, sql, new MySqlParameter("@mobile", receiver));
            return result != DBNull.Value && result != null;
        }

        /// <summary>
        /// 是否存在于邮箱黑名单
        /// </summary>
        /// <returns></returns>
        private static bool IsExistsMailBlackList(string connectionString, string receiver)
        {
            var sql = @"select mail_address as mail from b2b_support.bizp_message_email_blacklist where is_valid= 1 and ifnull(mail_address,'')<>'' and mail_address=@mail;";
            object result = MySQLHelper.ExecuteScalar(connectionString, sql, new MySqlParameter("@mail", receiver));
            return result != DBNull.Value && result != null;
        }

        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="messageTypeCode"></param>
        /// <param name="title"></param>
        /// <param name="sender"></param>
        /// <param name="senderId"></param>
        /// <param name="channel"></param>
        /// <param name="receiver"></param>
        /// <param name="content"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool SendMessageQueue(string connectionString, string messageTypeCode, string title, string sender,
    string senderId, string channel, string receiver, string content, string remark)
        {
            string queueId = Convert.ToString(IdGeneration.NewSequentialId(connectionString, MessageQueueKey));
            string batchId = string.Format("{0:D7}", IdGeneration.NewSequentialId(connectionString, MessageBatchKey));
            return SendMessageQueue(connectionString, queueId, messageTypeCode, title, batchId, sender, senderId,
                channel, receiver, content, null, remark, 1, 0);
        }

        /// <summary>
        /// 重构SendMessageQueue-便于监控
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="messageTypeCode"></param>
        /// <param name="title"></param>
        /// <param name="sender"></param>
        /// <param name="senderId"></param>
        /// <param name="channel"></param>
        /// <param name="receiver"></param>
        /// <param name="content"></param>
        /// <param name="remark"></param>
        /// <param name="monitor_batch">监控批次号</param>
        /// <param name="userId">用户ID</param>
        /// <param name="wayInfo">发送渠道信息</param>
        /// <returns></returns>
        public static bool SendMessageQueue(string connectionString, string messageTypeCode, string title, string sender,
            string senderId, string channel, string receiver, string content, string remark, string monitor_batch, string userId, string wayInfo,string businessType)
        {
            string queueId = Convert.ToString(IdGeneration.NewSequentialId(connectionString, MessageQueueKey));
            string batchId = string.Format("{0:D7}", IdGeneration.NewSequentialId(connectionString, MessageBatchKey));
            return SendMessageQueue(connectionString, queueId, messageTypeCode, title, batchId, sender, senderId,
                channel, receiver, content, null, remark, 1, 0, monitor_batch, userId, wayInfo, businessType);
        }



        public static bool SendOpMessageByMessageTemplateId(string connectionString, string messageTypeCode, string channel, string sender,
            string senderId, string receiver, string templateId, Dictionary<string, string> dicParams)
        {
            DataTable dt = QuartzExtScheduler.Common.MessageTemplate.GetOpMessageTemplate(connectionString, templateId);
            if (dt != null && dt.Rows.Count > 0)
            {
                string title = Convert.ToString(dt.Rows[0]["message_title"]);
                string content = Convert.ToString(dt.Rows[0]["content"]);
                if (dicParams != null && dicParams.Count > 0)
                {
                    title = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(title, dicParams);
                    content = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(content, dicParams);
                }
                return SendMessageQueue(connectionString, messageTypeCode, title, sender, senderId, channel, receiver, content, "");
            }

            return false;
        }

        /// <summary>
        /// 重构SendOpMessageByMessageTemplateId-便于监控
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="messageTypeCode"></param>
        /// <param name="channel"></param>
        /// <param name="sender"></param>
        /// <param name="senderId"></param>
        /// <param name="receiver"></param>
        /// <param name="templateId"></param>
        /// <param name="dicParams"></param>
        /// <param name="monitor_batch"></param>
        /// <param name="userId"></param>
        /// <param name="wayInfo"></param>
        /// <returns></returns>
        public static bool SendOpMessageByMessageTemplateId(string connectionString, string messageTypeCode, string channel, string sender,
            string senderId, string receiver, string templateId, Dictionary<string, string> dicParams, string monitor_batch, string userId, string wayInfo, string businessType, string remark)
        {
            DataTable dt = QuartzExtScheduler.Common.MessageTemplate.GetOpMessageTemplate(connectionString, templateId);
            if (dt != null && dt.Rows.Count > 0)
            {
                string title = Convert.ToString(dt.Rows[0]["message_title"]);
                string content = Convert.ToString(dt.Rows[0]["content"]);
                if (dicParams != null && dicParams.Count > 0)
                {
                    title = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(title, dicParams);
                    content = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(content, dicParams);
                }
                return SendMessageQueue(connectionString, messageTypeCode, title, sender, senderId, channel, receiver, content, remark, monitor_batch, userId, wayInfo, businessType);
            }

            return false;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="receiverId"></param>
        /// <param name="receiverName"></param>
        /// <returns></returns>
        public static bool SendSMS(string connectionString, string title, string message, string receiverId, string receiverName)
        {
            //系统消息
            string messageTypeCode = "200";
            string sender = "admin";
            string senderId = "admin";
            //短信
            string channel = "3";
            var receiver = string.Format("[{{\"addrMap\":{{{0}:\"{1}\"}},\"receiver\":\"{2}\",\"receiverId\":\"{1}\"}}]", channel, receiverId, receiverName);
            return SendMessageQueue(connectionString, messageTypeCode, title, sender, senderId, channel, receiver,
                message, null);
        }
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public static bool SendSMS(string connectionString, string title, string message, string receiverId)
        {
            return SendSMS(connectionString, title, message, receiverId, receiverId);
        }

        /// <summary>
        /// 根据模板ID和参数获取邮件标题和内容
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="templateId"></param>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public static MessageMain GetReplaceParametersMessageMainById(string connectionString, string templateId, Dictionary<string, string> dicParams)
        {
            MessageMain result=new MessageMain();

            DataTable dt = QuartzExtScheduler.Common.MessageTemplate.GetOpMessageTemplate(connectionString, templateId);
            if (dt != null && dt.Rows.Count > 0)
            {
                result.Title = Convert.ToString(dt.Rows[0]["message_title"]) ;
                result.Content = Convert.ToString(dt.Rows[0]["content"]);  

                if (dicParams != null && dicParams.Count > 0)
                {
                    result.Title = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(result.Title, dicParams);
                    result.Content = QuartzExtScheduler.Common.MessageTemplate.ReplaceVariable(result.Content, dicParams);
                }
            }
            return result;
        }

        /// <summary>
        /// 调用接口发送数据
        /// </summary>
        /// <param name="entitys"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        public static bool SendMessageService(List<PushEntity> entitys, string batchNo="")
        {
            entitys = CleanPushEntity(entitys);

            if (entitys.Count == 0)
            {
                return true;
            }
            string url = ConfigurationManager.AppSettings["MessageSendingInterface"];
            var model = new { Entitys = entitys, BatchNo = batchNo };
            var result = new JsonRequest().Post(url, model);
            Dictionary<string, string> resultDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            return resultDic["Result"] == "true";
        }

        /// <summary>
        /// 短地址
        /// </summary>
        /// <param name="url"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public static string GetShortUrl(string url, string memo)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("获取短地址错误");
            }

            string apiUrl = ConfigurationManager.AppSettings["shortUrlApi"];

            string result = string.Empty;
            try
            {
                result = new JsonRequest().Get(string.Format(apiUrl, url, "Admin", memo));
            }
            catch (Exception)
            {
                throw new Exception("获取短地址错误");
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("获取短地址错误");
            }

            Dictionary<string, string> data = null;
            try
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            }
            catch
            {
                throw new Exception("获取短地址错误");
            }

            if (data != null)
            {
                if (data.ContainsKey("result") && data["result"].ToLower() == "true")
                {
                    string temp = ConfigurationManager.AppSettings["Server_RootDomain"] + "/" + data["shortUrl"];
                    return temp;
                }
            }

            throw new Exception("获取短地址错误");
        }

        /// <summary>
        /// 清洗推送列表
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public static List<PushEntity> CleanPushEntity(List<PushEntity> entitys)
        {
            #region 剔除未替换掉参数的短信、微信
            List<PushEntity> real_pushList = new List<PushEntity>();

            foreach (var push in entitys)
            {
                switch (push.SendType)
                {
                    case SendType.EmailQueue:
                        real_pushList.Add(push);
                        break;
                    case SendType.SmsQueue:
                    case SendType.WxCustomQueue:
                    case SendType.WxCustomNewsQueue:
                        if (!push.SendContent.Contains("#"))
                        {
                            real_pushList.Add(push);
                        }
                        else
                        {
                            string param = GetNotReplacedParams(push.SendContent);
                            _log.Error(string.Format("{0}--短信消息模板有内容参数：{1}未被替换！", push.remark, param));
                        }
                        break;
                    case SendType.WxQueue:
                        if (!push.SendContent.Replace("#0080FF", "").Replace("#000000", "").Replace("#173177", "").Contains("#") && !push.SendTitle.Contains("#"))
                        {
                            real_pushList.Add(push);
                        }
                        else
                        {
                            string param = GetNotReplacedParams(push.SendContent.Replace("#0080FF", "").Replace("#000000", "").Replace("#173177", ""));
                            _log.Error(string.Format("{0}--微信消息模板有内容参数：{1}未被替换！", push.remark, param));
                        }
                        break;
                }
            }
            #endregion
            return real_pushList;
        }

        private static string GetNotReplacedParams(string content)
        {
            Regex r = new Regex("#([^#]*)#");
            MatchCollection mc = r.Matches(content);
            string param = string.Empty;
            foreach (Match c in mc)
            {
                param += c.Value + "、";
            }
            return param.Trim('、');
        }
    }
}

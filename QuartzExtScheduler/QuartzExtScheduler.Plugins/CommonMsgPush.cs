using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Common.Logging;
using QuartzExtScheduler.DataAccess;
using QuartzExtScheduler.Utility.Core;
using QuartzExtScheduler.Utility.Data;
using MySql.Data.MySqlClient;
using Quartz;
using QuartzExtScheduler.Entity.CommonMsgPushEntity;
using System.Xml.Linq;
using QuartzExtScheduler.Entity;
using QuartzExtScheduler.Entity.BizPushEntity;

namespace QuartzExtScheduler.Plugins
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class CommonMsgPush : IJob
    {
        #region Init
        private ILog _log = LogManager.GetCurrentClassLogger();
        private string connectionString = string.Empty;
        private string appId = string.Empty;
        private string appSecret = string.Empty;
        private string appToken = string.Empty;
        private readonly string _wx_token = ConfigurationManager.AppSettings["wxToken"];
        private string _template_name = "任务处理通知"; //微信中使用，暂时还是不能修改
        private string _task_xml_path = string.Empty;
        private string _business_type = string.Empty;
        private string _weekend_push = string.Empty;
        private string _send_control_type = string.Empty;
        private string _sms_send_control_type = string.Empty;
        private string _email_send_control_type = string.Empty;
        private string _summaryTaskName = string.Empty;

        private IDictionary<string, string> globalConfig = new Dictionary<string, string>();
        public void Execute(IJobExecutionContext context)
        {

            try
            {
                this.Init(context);
                this._log.Info(_summaryTaskName + "开始执行");
                if (_weekend_push == "0")
                {
                    //周六周日不推送消息                      
                    if (!IsWorkDay(DateTime.Now))
                    {
                        _log.Info("当前为非工作日，已结束。");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                this._log.ErrorFormat(_summaryTaskName + "初始化异常！{0}", ex);
            }

            if (string.IsNullOrWhiteSpace(this.connectionString))
            {
                this._log.Info("数据库连接字符串未配置，已结束。");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.appId))
            {
                this._log.Info("appId未配置，已结束。");
                return;
            }
            try
            {
                AnalyzeTask();
            }
            catch (Exception ex)
            {
                this._log.Error(_summaryTaskName + "执行异常！" + ex);
            }
            this._log.Info(_summaryTaskName + "结束执行");
        }

        /// <summary>
        /// 解析任务配置文件
        /// </summary>
        private void AnalyzeTask()
        {
            string taskXML = AppDomain.CurrentDomain.BaseDirectory + @"TaskMsgPushConfig\" + @_task_xml_path;
            XElement xml = XElement.Load(taskXML);
            var taskList = from p in xml.Element("items").Elements("item")
                           select new TaskEntity
                           {
                               Description = p.Element("description").Value.Trim(),
                               Message_Template = new List<MessageTemplate>()
                                {
                                    new MessageTemplate{ 
                                        MessageType="sms", 
                                        Template_ID=p.Element("message-template-id").Element("sms").Value.Trim(),
                                        Only_One= p.Element("message-template-id").Element("sms").Attribute("priority")==null? null:p.Element("message-template-id").Element("sms").Attribute("priority").Value.Trim()},
                                    new MessageTemplate{ 
                                        MessageType="email", 
                                        Template_ID=p.Element("message-template-id").Element("email").Value.Trim(),
                                        Only_One= p.Element("message-template-id").Element("email").Attribute("priority")==null? null:p.Element("message-template-id").Element("email").Attribute("priority").Value.Trim()},
                                    new MessageTemplate{ 
                                        MessageType="wechat", 
                                        Template_ID=p.Element("message-template-id").Element("wechat").Value.Trim(),
                                        Only_One= p.Element("message-template-id").Element("wechat").Attribute("priority")==null? null:p.Element("message-template-id").Element("wechat").Attribute("priority").Value.Trim(),
                                        Wechat_Template_Name=p.Element("message-template-id").Element("wechat").Attribute("wechat-template-name")==null? null:p.Element("message-template-id").Element("wechat").Attribute("wechat-template-name").Value.Trim()},
                                    new MessageTemplate{ 
                                        MessageType="wechat-custom", 
                                        Template_ID=p.Element("message-template-id").Element("wechat-custom")== null?string.Empty:p.Element("message-template-id").Element("wechat-custom").Value.Trim(),
                                        Only_One= p.Element("message-template-id").Element("wechat-custom")== null?null:
                                            p.Element("message-template-id").Element("wechat-custom").Attribute("priority")==null? null:p.Element("message-template-id").Element("wechat-custom").Attribute("priority").Value.Trim()},
                                    new MessageTemplate{ 
                                        MessageType="wechat-custom-news", 
                                        Template_ID=p.Element("message-template-id").Element("wechat-custom-news")== null?string.Empty:p.Element("message-template-id").Element("wechat-custom-news").Value.Trim(),
                                        Only_One= p.Element("message-template-id").Element("wechat-custom-news")== null?null:
                                            p.Element("message-template-id").Element("wechat-custom-news").Attribute("priority")==null? null:p.Element("message-template-id").Element("wechat-custom-news").Attribute("priority").Value.Trim()}
                                },
                               Scope_Entity = (from t in p.Element("content").Elements("sql-item")
                                               where t.Attribute("type").Value == "scope"
                                               select new SqlEntity
                                               {
                                                   ID = t.Attribute("id").Value.Trim(),
                                                   Sql_String = t.Element("sql").Value.Trim(),
                                                   Description = t.Element("description").Value.Trim(),
                                                   Sql_Parameters = t.Element("sql-parameters") == null ? null : (from a in t.Element("sql-parameters").Elements("parameter")
                                                                                                                  select new ParameterEntity
                                                                                                                  {
                                                                                                                      Key = a.Element("key").Value.Trim(),
                                                                                                                      Value = a.Element("value").Value.Trim(),
                                                                                                                      Source = a.Element("value").Attribute("source").Value.Trim(),
                                                                                                                      ConvertShortURL = a.Element("value").Attribute("convert-short-url") == null ? false : true
                                                                                                                  }).ToList(),
                                                   Content_Parameters = t.Element("content-parameters") == null ? null : (from a in t.Element("content-parameters").Elements("parameter")
                                                                                                                          select new ParameterEntity
                                                                                                                          {
                                                                                                                              Key = a.Element("key").Value.Trim(),
                                                                                                                              Value = a.Element("value").Value.Trim(),
                                                                                                                              Source = a.Element("value").Attribute("source").Value.Trim(),
                                                                                                                              ConvertShortURL = a.Element("value").Attribute("convert-short-url") == null ? false : true
                                                                                                                          }).ToList()
                                               }).FirstOrDefault(),
                               Content_Entity = (from t in p.Element("content").Elements("sql-item")
                                                 where t.Attribute("type").Value == "content"
                                                 select new SqlEntity
                                                 {
                                                     ID = t.Attribute("id").Value.Trim(),
                                                     Sql_String = t.Element("sql").Value.Trim(),
                                                     Description = t.Element("description").Value.Trim(),
                                                     Sql_Parameters = t.Element("sql-parameters") == null ? null : (from a in t.Element("sql-parameters").Elements("parameter")
                                                                                                                    select new ParameterEntity
                                                                                                                    {
                                                                                                                        Key = a.Element("key").Value.Trim(),
                                                                                                                        Value = a.Element("value").Value.Trim(),
                                                                                                                        Source = a.Element("value").Attribute("source").Value.Trim(),
                                                                                                                        ConvertShortURL = a.Element("value").Attribute("convert-short-url") == null ? false : true
                                                                                                                    }).ToList(),
                                                     Content_Parameters = t.Element("content-parameters") == null ? null : (from a in t.Element("content-parameters").Elements("parameter")
                                                                                                                            select new ParameterEntity
                                                                                                                            {
                                                                                                                                Key = a.Element("key").Value.Trim(),
                                                                                                                                Value = a.Element("value").Value.Trim(),
                                                                                                                                Source = a.Element("value").Attribute("source").Value.Trim(),
                                                                                                                                ConvertShortURL = a.Element("value").Attribute("convert-short-url") == null ? false : true
                                                                                                                            }).ToList()
                                                 }).ToList()
                           };
            foreach (var task in taskList)
            {
                if (task.Scope_Entity == null && task.Scope_Entity.Sql_String == string.Empty)
                {
                    this._log.Error(string.Format("{0}--scope脚本配置错误！", task.Description));
                    continue;
                }
                Process(task);
            }
        }

        private void Init(IJobExecutionContext context)
        {
            string connectionName = context.JobDetail.JobDataMap["ConnectionName"].GetString();
            _task_xml_path = context.JobDetail.JobDataMap["TaskXMLPath"].GetString();
            _business_type = context.JobDetail.JobDataMap["BusinessType"].GetString();
            _weekend_push = context.JobDetail.JobDataMap["WeekendPush"].GetString();
            _send_control_type = context.JobDetail.JobDataMap["SendControlType"].GetString();
            _sms_send_control_type = context.JobDetail.JobDataMap["SMSSendControlType"].GetString();
            _email_send_control_type = context.JobDetail.JobDataMap["EmailSendControlType"].GetString();
            _summaryTaskName = context.JobDetail.JobDataMap["Name"].GetString();

            appId = ConfigurationManager.AppSettings["AppId"];
            appSecret = ConfigurationManager.AppSettings["AppSecret"];
            List<string> allKey = ConfigurationManager.AppSettings.AllKeys.ToList();
            foreach (var key in allKey)
            {
                globalConfig.Add(key.ToUpper(), ConfigurationManager.AppSettings[key]);
            }

            if (connectionName.Length == 0)
            {
                connectionName = "Default";
            }
            connectionString = ConfigurationManager.ConnectionStrings[connectionName].GetString();
        }
        #endregion

        #region Process
        private void Process(TaskEntity task)
        {
            long batchId = IdGeneration.NewSequentialId(this.connectionString, "message_service_batch_generation");
            string MESSAGE_Template = string.Empty; //信息模板
            string PUSH_MESSAGE_BUSINESS_TYPE = string.Empty; //模板类型 需要配置在表 dotnet_operation.op_push_msg_business_type中
            DataTable dtUsers = GetUserModel(task.Scope_Entity);
            if (dtUsers == null || dtUsers.Rows.Count == 0)
            {
                this._log.Info(string.Format("{0}--推送用户为空！", task.Description));
                return;
            }

            Dictionary<string, string> dicParams;
            IList<PushEntity> pushList;
            DataRowCollection drUsers = dtUsers.Rows;
            string template_SMS = string.Empty;
            string template_Email = string.Empty;
            string tempalte_Wechat = string.Empty;
            string template_WechatCustom = string.Empty;
            string template_WechatCustomNews = string.Empty;

            string user_id = string.Empty;
            string mobile = string.Empty;
            string mail = string.Empty;
            string open_id = string.Empty;
            bool has_config_SMS = true;
            bool has_config_Email = true;
            bool has_config_Wechat = true;
            bool has_config_WechatCustom = true;
            bool has_config_WechatCustomNews = true;

            string wechat_tamplate_name=string.Empty;

            #region 查看是否配置信息模板 & 构造优先级推送列表

            Dictionary<string, int> dicOnlyOne = new Dictionary<string, int>();
            foreach (var dic in task.Message_Template)
            {
                switch (dic.MessageType)
                {
                    case "sms":
                        if (dic.Template_ID.Trim() != string.Empty)
                        {
                            template_SMS = dic.Template_ID.Trim();
                            if (!string.IsNullOrWhiteSpace(dic.Only_One))
                            {
                                dicOnlyOne.Add("sms", Convert.ToInt32(dic.Only_One));
                            }
                        }
                        else
                        {
                            has_config_SMS = false;
                        }
                        break;
                    case "email":
                        if (dic.Template_ID.Trim() != string.Empty)
                        {
                            template_Email = dic.Template_ID.Trim();
                            if (!string.IsNullOrWhiteSpace(dic.Only_One))
                            {
                                dicOnlyOne.Add("email", Convert.ToInt32(dic.Only_One));
                            }
                        }
                        else
                        {
                            has_config_Email = false;
                        }
                        break;
                    case "wechat":
                        if (dic.Template_ID.Trim() != string.Empty)
                        {
                            tempalte_Wechat = dic.Template_ID.Trim();
                            if (!string.IsNullOrWhiteSpace(dic.Only_One))
                            {
                                dicOnlyOne.Add("wechat", Convert.ToInt32(dic.Only_One));
                            }
                        }
                        else
                        {
                            has_config_Wechat = false;
                        }

                        if(!string.IsNullOrWhiteSpace(dic.Wechat_Template_Name))
                        {
                            wechat_tamplate_name = dic.Wechat_Template_Name;
                        }
                        break;
                    case "wechat-custom":
                        if (dic.Template_ID.Trim() != string.Empty)
                        {
                            template_WechatCustom = dic.Template_ID.Trim();
                            if (!string.IsNullOrWhiteSpace(dic.Only_One))
                            {
                                dicOnlyOne.Add("wechatCustom", Convert.ToInt32(dic.Only_One));
                            }
                        }
                        else
                        {
                            has_config_WechatCustom = false;
                        }
                        break;
                    case "wechat-custom-news":
                        if (dic.Template_ID.Trim() != string.Empty)
                        {
                            template_WechatCustomNews = dic.Template_ID.Trim();
                            if (!string.IsNullOrWhiteSpace(dic.Only_One))
                            {
                                dicOnlyOne.Add("wechatCustomNews", Convert.ToInt32(dic.Only_One));
                            }
                        }
                        else
                        {
                            has_config_WechatCustomNews = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            List<string> listPrioritySend = new List<string>();
            if (dicOnlyOne.Count > 1)
            {
                listPrioritySend = dicOnlyOne.OrderBy(x => x.Value).Select(a => a.Key).ToList();
            }

            #endregion

            for (int i = 0; i < drUsers.Count; i++)
            {
                pushList = new List<PushEntity>();
                bool can_SMS = has_config_SMS;
                bool can_Email = has_config_Email;
                bool can_Wechat = has_config_Wechat;
                bool can_WechatCustom = has_config_WechatCustom;
                bool can_WechatCustomNews = has_config_WechatCustomNews;
                if (!dtUsers.Columns.Contains("user_id") || drUsers[i]["user_id"] == null || drUsers[i]["user_id"].ToString().Trim() == string.Empty)
                {
                    this._log.Error(string.Format("{0}--推送用户user_id为空！", task.Description));
                    return;
                }
                user_id = drUsers[i]["user_id"].ToString();

                #region 验证是否存在消息Receiver
                //配置了短信模板
                if (has_config_SMS)
                {
                    if (!dtUsers.Columns.Contains("mobile"))
                    {
                        this._log.Error(string.Format("{0}--sms节点有值，必需包含字段【mobile】！", task.Description));
                        return;
                    }
                    else
                    {
                        if (drUsers[i]["mobile"] == null || drUsers[i]["mobile"].ToString().Trim() == string.Empty)
                        {
                            //this._log.Info(string.Format("{0}--sms节点有值，user_id={1}字段【mobile】为空，本记录取消推送", task.Description, user_id));
                            can_SMS = false;
                        }
                        else
                        {
                            mobile = drUsers[i]["mobile"].ToString().Trim();
                            can_SMS = true;
                        }
                    }
                }
                //配置了邮件模板
                if (has_config_Email)
                {
                    if (!dtUsers.Columns.Contains("mail"))
                    {
                        this._log.Error(string.Format("{0}--email节点有值，必需包含字段【mail】！", task.Description));
                        return;
                    }
                    else
                    {
                        if (drUsers[i]["mail"] == null || drUsers[i]["mail"].ToString().Trim() == string.Empty)
                        {
                            //this._log.Info(string.Format("{0}--email节点有值，user_id={1}字段【mail】为空，本记录取消推送", task.Description, user_id));
                            can_Email = false;
                        }
                        else
                        {
                            mail = drUsers[i]["mail"].ToString().Trim();
                            can_Email = true;
                        }
                    }
                }
                //配置了wechat模板
                if (has_config_Wechat)
                {
                    if (!dtUsers.Columns.Contains("openid"))
                    {
                        this._log.Error(string.Format("{0}--wechat节点有值，必需包含字段【openid】！", task.Description));
                        return;
                    }
                    else
                    {
                        if (drUsers[i]["openid"] == null || drUsers[i]["openid"].ToString().Trim() == string.Empty)
                        {
                            //this._log.Info(string.Format("{0}--wechat节点有值，user_id={1}字段【openid】为空，本记录取消推送", task.Description, user_id));
                            can_Wechat = false;
                        }
                        else
                        {
                            open_id = drUsers[i]["openid"].ToString().Trim();
                            can_Wechat = true;
                        }
                    }
                }
                //配置了wechat-custom模板
                if (has_config_WechatCustom)
                {
                    if (!dtUsers.Columns.Contains("openid"))
                    {
                        this._log.Error(string.Format("{0}--wechat-custom节点有值，必需包含字段【openid】！", task.Description));
                        return;
                    }
                    else
                    {
                        if (drUsers[i]["openid"] == null || drUsers[i]["openid"].ToString().Trim() == string.Empty)
                        {
                            //this._log.Info(string.Format("{0}--wechat节点有值，user_id={1}字段【openid】为空，本记录取消推送", task.Description, user_id));
                            can_WechatCustom = false;
                        }
                        else
                        {
                            open_id = drUsers[i]["openid"].ToString().Trim();
                            can_WechatCustom = true;
                        }
                    }
                }
                //配置了wechat-custom-news模板
                if (has_config_WechatCustomNews)
                {
                    if (!dtUsers.Columns.Contains("openid"))
                    {
                        this._log.Error(string.Format("{0}--wechat-custom-news节点有值，必需包含字段【openid】！", task.Description));
                        return;
                    }
                    else
                    {
                        if (drUsers[i]["openid"] == null || drUsers[i]["openid"].ToString().Trim() == string.Empty)
                        {
                            //this._log.Info(string.Format("{0}--wechat节点有值，user_id={1}字段【openid】为空，本记录取消推送", task.Description, user_id));
                            can_WechatCustomNews = false;
                        }
                        else
                        {
                            open_id = drUsers[i]["openid"].ToString().Trim();
                            can_WechatCustomNews = true;
                        }
                    }
                }

                #endregion

                #region 替换内容参数
                dicParams = new Dictionary<string, string>();
                bool can_Send = true;
                //替换Scope中内容参数
                if (task.Scope_Entity == null)
                {
                    this._log.Error(string.Format("{0}--Scope配置为空！", task.Description));
                    return;
                }
                if (task.Scope_Entity.Content_Parameters != null && task.Scope_Entity.Content_Parameters.Count > 0)
                {
                    foreach (var scope_param in task.Scope_Entity.Content_Parameters)
                    {
                        if (scope_param.Source == "entity")
                        {
                            if (scope_param.ConvertShortURL)
                            {
                                try
                                {
                                    dicParams.Add(scope_param.Key.Trim('#'), MessageQueue.GetShortUrl(drUsers[i][scope_param.Value].ToString(), task.Description));
                                }
                                catch (Exception ex)
                                {
                                    this._log.Error(string.Format("{0}--短地址请求错误！--{1}", task.Description, ex.Message));
                                }
                            }
                            else
                            {
                                dicParams.Add(scope_param.Key.Trim('#'), drUsers[i][scope_param.Value].ToString());
                            }
                        }
                        else if (scope_param.Source.ToLower() == "system")
                        {
                            if (globalConfig.ContainsKey(scope_param.Value))
                            {
                                dicParams.Add(scope_param.Key.Trim('#'), globalConfig[scope_param.Value]);
                            }
                            else
                            {
                                this._log.Error(string.Format("{0}--配置项-{1}-未找到对应值！", task.Description, scope_param.Key));
                                return;
                            }
                        }
                        else
                        {
                            //待扩展
                        }
                    }
                }
                Dictionary<string, string> dicContentParams;
                if (task.Content_Entity != null)
                {
                    foreach (SqlEntity contentEntity in task.Content_Entity)
                    {
                        string sql_string = contentEntity.Sql_String;
                        dicContentParams = new Dictionary<string, string>();
                        if (contentEntity.Sql_Parameters != null && contentEntity.Sql_Parameters.Count > 0)
                        {
                            foreach (var par in contentEntity.Sql_Parameters)
                            {
                                if (par.Source.ToLower() == "entity")
                                {
                                    dicContentParams.Add(par.Key, drUsers[i][par.Value].ToString());
                                }
                                else
                                {
                                    dicContentParams.Add(par.Key, globalConfig.ContainsKey(par.Value) ? globalConfig[par.Value] : string.Empty);
                                }
                            }
                        }
                        DataTable dtFitContent = GetFitContent(sql_string, dicContentParams);
                        if (dtFitContent == null || dtFitContent.Rows.Count == 0)
                        {
                            this._log.Info(string.Format("{0}--user_id={1}--{2}记录为空！", task.Description, user_id, contentEntity.Description));
                            can_Send = false;
                            break;
                        }
                        else
                        {
                            //替换内容SQL中内容参数
                            if (contentEntity.Content_Parameters != null & contentEntity.Content_Parameters.Count > 0)
                            {
                                foreach (var contentParam in contentEntity.Content_Parameters)
                                {
                                    if (contentParam.Source.ToLower() == "entity")
                                    {
                                        if (contentParam.ConvertShortURL)
                                        {
                                            try
                                            {
                                                //外部请求一定加上异常处理，立碑于20160816 19:36
                                                dicParams.Add(contentParam.Key.Trim('#'), MessageQueue.GetShortUrl(dtFitContent.Rows[0][contentParam.Value].ToString(), task.Description));
                                            }
                                            catch (Exception ex)
                                            {
                                                this._log.Error(string.Format("{0}--短地址请求错误！--{1}", task.Description, ex.Message));
                                            }
                                        }
                                        else
                                        {
                                            dicParams.Add(contentParam.Key.Trim('#'), dtFitContent.Rows[0][contentParam.Value].ToString());
                                        }
                                    }
                                    else if (contentParam.Source.ToLower() == "system")
                                    {
                                        if (globalConfig.ContainsKey(contentParam.Value))
                                        {
                                            dicParams.Add(contentParam.Key.Trim('#'), globalConfig[contentParam.Value]);
                                        }
                                        else
                                        {
                                            this._log.Error(string.Format("{0}--配置项-{1}-未找到对应值！", task.Description, contentParam.Key));
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        //待扩展
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 内容参数为空或0不发送

                foreach (var a in dicParams)
                {
                    if (string.IsNullOrWhiteSpace(a.Value) || a.Value == "0")
                    {
                        can_Send = false;
                    }
                }
                if (!can_Send)
                {
                    //this._log.Info(string.Format("{0}--user_id={1}--内容参数存在取值为空或0的数据，取消推送！", task.Description, user_id));
                    continue;
                }
                #endregion

                //优先级推送设置<2 其实为0
                if (listPrioritySend.Count > 1)
                {
                    //优先级发送，第1条满足不发后面的，第1条不满足查看优先级第2的，依次类推
                    foreach (string liSend in listPrioritySend)//listPrioritySend已经按优先级排过序
                    {
                        if (liSend == SendMessageConfigType.email.ToString())
                        {
                            if (can_Email)
                            {
                                MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_Email, dicParams);
                                pushList.Add(new PushEntity
                                {
                                    UserID = user_id,
                                    Email = mail,
                                    SendType = SendType.EmailQueue,
                                    Business_Type = this._business_type,
                                    Send_Control_Type = this._send_control_type,
                                    SMS_Send_Control_Type = this._sms_send_control_type,
                                    Email_Send_Control_Type = this._email_send_control_type,
                                    SendContent = messageMain.Content,
                                    SendTitle = messageMain.Title,
                                    Sender = "Admin",
                                    remark = task.Description
                                });
                                break;
                            }
                            continue;
                        }
                        else if (liSend == SendMessageConfigType.sms.ToString())
                        {
                            if (can_SMS)
                            {
                                MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_SMS, dicParams);
                                pushList.Add(new PushEntity
                                {
                                    UserID = user_id,
                                    Mobile = mobile,
                                    SendType = SendType.SmsQueue,
                                    Business_Type = this._business_type,
                                    Send_Control_Type = this._send_control_type,
                                    SMS_Send_Control_Type = this._sms_send_control_type,
                                    Email_Send_Control_Type = this._email_send_control_type,
                                    SendContent = messageMain.Content,
                                    SendTitle = this._template_name,
                                    Sender = "Admin",
                                    remark = task.Description
                                });
                                break;
                            }
                            continue;
                        }
                        else if (liSend == SendMessageConfigType.wechat.ToString())
                        {
                            if (can_Wechat)
                            {
                                MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, tempalte_Wechat, dicParams);
                                pushList.Add(new PushEntity
                                {
                                    UserID = user_id,
                                    OpenID = open_id,
                                    SendType = SendType.WxQueue,
                                    Business_Type = this._business_type,
                                    Send_Control_Type = this._send_control_type,
                                    SMS_Send_Control_Type = this._sms_send_control_type,
                                    Email_Send_Control_Type = this._email_send_control_type,
                                    SendContent = messageMain.Content,
                                    SendTitle = string.IsNullOrWhiteSpace(wechat_tamplate_name) ? this._template_name : wechat_tamplate_name,
                                    Sender = this._wx_token,
                                    remark = task.Description
                                });
                                break;
                            }
                            continue;
                        }
                        else if (liSend == SendMessageConfigType.wechatCustom.ToString())
                        {
                            if (can_WechatCustom)
                            {
                                MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustom, dicParams);
                                pushList.Add(new PushEntity
                                {
                                    UserID = user_id,
                                    OpenID = open_id,
                                    SendType = SendType.WxCustomQueue,
                                    Business_Type = this._business_type,
                                    Send_Control_Type = this._send_control_type,
                                    SMS_Send_Control_Type = this._sms_send_control_type,
                                    Email_Send_Control_Type = this._email_send_control_type,
                                    SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                                    SendTitle = this._template_name,
                                    Sender = this._wx_token,
                                    remark = task.Description
                                });
                                break;
                            }
                            continue;
                        }
                        else if (liSend == SendMessageConfigType.wechatCustomNews.ToString())
                        {
                            if (can_WechatCustomNews)
                            {
                                MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustomNews, dicParams);
                                pushList.Add(new PushEntity
                                {
                                    UserID = user_id,
                                    OpenID = open_id,
                                    SendType = SendType.WxCustomNewsQueue,
                                    Business_Type = this._business_type,
                                    Send_Control_Type = this._send_control_type,
                                    SMS_Send_Control_Type = this._sms_send_control_type,
                                    Email_Send_Control_Type = this._email_send_control_type,
                                    SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                                    SendTitle = this._template_name,
                                    Sender = this._wx_token,
                                    remark = task.Description
                                });
                                break;
                            }
                            continue;
                        }
                        else
                        {
                            //待扩展
                        }
                    }


                    //没有设置优先级
                    if (!listPrioritySend.Contains(SendMessageConfigType.email.ToString()) && can_Email)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_Email, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            Email = mail,
                            SendType = SendType.EmailQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = messageMain.Title,
                            Sender = "Admin",
                            remark = task.Description
                        });
                    }
                    if (!listPrioritySend.Contains(SendMessageConfigType.sms.ToString()) && can_SMS)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_SMS, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            Mobile = mobile,
                            SendType = SendType.SmsQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = this._template_name,
                            Sender = "Admin",
                            remark = task.Description
                        });
                    }
                    if (!listPrioritySend.Contains(SendMessageConfigType.wechat.ToString()) && can_Wechat)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, tempalte_Wechat, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = string.IsNullOrWhiteSpace(wechat_tamplate_name) ? this._template_name : wechat_tamplate_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                    if (!listPrioritySend.Contains(SendMessageConfigType.wechatCustom.ToString()) && can_WechatCustom)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustom, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxCustomQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                            SendTitle = this._template_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                    if (!listPrioritySend.Contains(SendMessageConfigType.wechatCustomNews.ToString()) && can_WechatCustomNews)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustomNews, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxCustomNewsQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                            SendTitle = this._template_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                }
                else
                {
                    if (can_Email)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_Email, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            Email = mail,
                            SendType = SendType.EmailQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = messageMain.Title,
                            Sender = "Admin",
                            remark = task.Description
                        });
                    }
                    if (can_SMS)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_SMS, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            Mobile = mobile,
                            SendType = SendType.SmsQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = this._template_name,
                            Sender = "Admin",
                            remark = task.Description
                        });
                    }
                    if (can_Wechat)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, tempalte_Wechat, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content,
                            SendTitle = string.IsNullOrWhiteSpace(wechat_tamplate_name) ? this._template_name : wechat_tamplate_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                    if (can_WechatCustom)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustom, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxCustomQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                            SendTitle = this._template_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                    if (can_WechatCustomNews)
                    {
                        MessageMain messageMain = MessageQueue.GetReplaceParametersMessageMainById(this.connectionString, template_WechatCustomNews, dicParams);
                        pushList.Add(new PushEntity
                        {
                            UserID = user_id,
                            OpenID = open_id,
                            SendType = SendType.WxCustomNewsQueue,
                            Business_Type = this._business_type,
                            Send_Control_Type = this._send_control_type,
                            SMS_Send_Control_Type = this._sms_send_control_type,
                            Email_Send_Control_Type = this._email_send_control_type,
                            SendContent = messageMain.Content.Replace(@"\r\n", "\n"),
                            SendTitle = this._template_name,
                            Sender = this._wx_token,
                            remark = task.Description
                        });
                    }
                }

                if (this._business_type == "5" || this._business_type == "6")
                {
                    // 记录日志
                    BizOppsLog log = new BizOppsLog();
                    log.Uid = Guid.NewGuid().ToString();
                    //log.CleanId = item.UID;
                    log.UserId = user_id;
                    //log.UserName = item.UserName;
                    //log.Mobile = item.Mobile;
                    //log.DeveloperName = developerNameList.Count > 0 ? developerNameList[0] : string.Empty;
                    log.CleanCreatedTime = DateTime.Now;
                    log.CleanCreatedBy = "admin";
                    log.CreatedTime = DateTime.Now;
                    log.CreatedBy = "admin";
                    log.TempateId = ((can_Email ? template_Email + "," : string.Empty) + (can_SMS ? template_SMS + "," : string.Empty) + (can_Wechat ? tempalte_Wechat : string.Empty)).Trim(',');
                    log.Chanel = ((can_Email ? "EmailQueue," : string.Empty) + (can_SMS ? "SmsQueue," : string.Empty) + (can_Wechat ? "WxQueue" : string.Empty)).Trim(',');
                    this.SaveBizOppsLog(log);
                }

                //发送消息
                if (pushList.Count > 0)
                {
                    bool result = MessageQueue.SendMessageService(pushList as List<PushEntity>, batchId.ToString());
                    if (!result)
                    {
                        this._log.Info(string.Format("{0}- user_id={1}发送异常:{2}", task.Description, user_id, result));
                    }
                }
            }
        }

        private DataTable GetUserModel(SqlEntity scope)
        {
            string sql = scope.Sql_String;
            List<MySqlParameter> listParams = new List<MySqlParameter>();
            if (scope.Sql_Parameters != null && scope.Sql_Parameters.Count > 0)
            {
                foreach (ParameterEntity param in scope.Sql_Parameters)
                {
                    listParams.Add(new MySqlParameter(param.Key, globalConfig[param.Value.ToUpper()]));
                }
                return MySQLHelper.ExecuteDataTable(connectionString, sql, listParams.ToArray());
            }
            else
            {
                return MySQLHelper.ExecuteDataTable(connectionString, sql);
            }
        }

        private DataTable GetFitContent(string contentSql, Dictionary<string, string> parms)
        {
            List<MySqlParameter> listParams = new List<MySqlParameter>();
            if (parms != null && parms.Count > 0)
            {
                foreach (var p in parms)
                {
                    listParams.Add(new MySqlParameter(p.Key, p.Value));
                }
                return MySQLHelper.ExecuteDataTable(connectionString, contentSql, listParams.ToArray());
            }
            else
            {
                return MySQLHelper.ExecuteDataTable(connectionString, contentSql);
            }
        }
        #endregion

        #region Common method
        /// <summary>
        /// 是否为工作日
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private bool IsWorkDay(DateTime date)
        {
            DayOfWeek dayOfWeek = date.DayOfWeek;
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 获取前N个工作日的日期
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        private DateTime GetPreDate(int days)
        {
            DateTime date = DateTime.Now;
            for (int i = days; i > 1; i--)
            {
                date = date.AddDays(-1);
                while (!IsWorkDay(date))
                {
                    date = date.AddDays(-1);
                }
            }

            return date;
        }

        private void SaveBizOppsLog(BizOppsLog model)
        {
            string sql = @"INSERT INTO `b2b_bidding`.`bid_recruit_biz_opps_log`(
			                uid,
			                clean_id,
			                user_id,
			                user_name,
			                mobile,
			                developer_name,
			                channel,
			                type,
			                template_id,
			                clean_created_time,
			                clean_created_by,
			                created_time,
			                created_by
		                )
		                VALUES
		                (
			                @uid,
			                @cleanId,
			                @userId,
			                @userName,
			                @mobile,
			                @developerName,
			                @channel,
			                @type,
			                @templateId,
			                @cleanCreatedTime,
			                @cleanCreatedBy,
			                @createdTime,
			                @createdBy
		                )";

            MySQLHelper.ExecuteNonQuery(this.connectionString, sql,
                new MySqlParameter("uid", model.Uid),
                new MySqlParameter("cleanId", model.CleanId),
                new MySqlParameter("userId", model.UserId),
                new MySqlParameter("userName", model.UserName),
                new MySqlParameter("mobile", model.Mobile),
                new MySqlParameter("developerName", model.DeveloperName),
                new MySqlParameter("channel", model.Chanel),
                new MySqlParameter("type", model.Type),
                new MySqlParameter("templateId", model.TempateId),
                new MySqlParameter("cleanCreatedTime", model.CleanCreatedTime),
                new MySqlParameter("cleanCreatedBy", model.CleanCreatedBy),
                new MySqlParameter("createdTime", model.CreatedTime),
                new MySqlParameter("createdBy", model.CreatedBy));
        }

        #endregion
    }
}
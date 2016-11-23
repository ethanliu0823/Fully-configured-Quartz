using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Common.Logging;
using Quartz;
using Quartz.Impl;
using System.Xml;
using QuartzExtScheduler.Plugins;

namespace QuartzExtScheduler.Service
{
    public partial class QuartzExtService : ServiceBase
    {
        private readonly ILog _log = LogManager.GetCurrentClassLogger();
        private ISchedulerFactory _factory;
        private IScheduler _scheduler;
        public QuartzExtService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("调度服务初始化...");
                var properties = new NameValueCollection();
                properties["quartz.plugin.triggHistory.type"] = "Quartz.Plugin.History.LoggingJobHistoryPlugin";
                properties["quartz.plugin.jobInitializer.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin";
                properties["quartz.plugin.jobInitializer.fileNames"] = AppDomain.CurrentDomain.BaseDirectory + "Jobs.xml";
                properties["quartz.plugin.jobInitializer.failOnFileNotFound"] = "true";
                properties["quartz.plugin.jobInitializer.scanInterval"] = "3600";
                _factory = new StdSchedulerFactory(properties);
                _scheduler = _factory.GetScheduler();
                _log.Info("调度服务初始化完成。");
                _log.Info("开始调度任务...");
                _scheduler.Start();
                _log.Info("调度任务已开始。");

                try
                {
                    GenerateTask<CommonMsgPush>("TaskMsgPushConfig//Summary.xml", "CommonMsgPush");
                    GenerateTask<CommonDataCleaner>("TaskDataCleanerConfig//Summary.xml", "CommonDataCleaner");
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("通用调度程序异常，原因：{0}", ex.Message);
                }
            }
            catch (Exception exception)
            {
                _log.ErrorFormat("调度服务初始化失败，原因：{0}", exception.Message);
            }
        }
        
        protected void GenerateTask<T>(string summaryPath, string jobName) where T : IJob
        {
            string summaryXML = AppDomain.CurrentDomain.BaseDirectory + summaryPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(summaryXML);
            XmlNode tasks = doc.SelectSingleNode("task-list");
            if (tasks != null)
            {
                List<string> task_key = new List<string>();
                foreach (XmlNode task in tasks.ChildNodes)
                {
                    string name = task.SelectSingleNode("name").InnerText.Trim();
                    if (!task_key.Contains(name))
                    {
                        task_key.Add(name);
                    }
                    else
                    {
                        _log.ErrorFormat("通用调度程序{1}初始化异常，原因：Summary文件中任务名称【{0}】已存在", name, jobName);

                        return;
                    }
                }

                foreach (XmlNode task in tasks.ChildNodes)
                {
                    string name = task.SelectSingleNode("name").InnerText.Trim();
                    string cron = task.SelectSingleNode("cron-expression").InnerText.Trim();
                    string child_path = task.SelectSingleNode("childPath").InnerText.Trim();
                    string connection_name = task.SelectSingleNode("connection-name").InnerText.Trim();
                    string description = task.SelectSingleNode("description").InnerText.Trim();
                    string business_type = string.Empty;
                    string weekend_push = string.Empty;
                    string send_control_type = string.Empty;
                    string sms_send_control_type = string.Empty;
                    string email_send_control_type = string.Empty;
                    if (jobName == "CommonMsgPush")
                    {
                        business_type = task.SelectSingleNode("business-type").InnerText.Trim();
                        weekend_push = task.SelectSingleNode("weekend-push").InnerText.Trim();
                        send_control_type = task.SelectSingleNode("send-control-type").InnerText.Trim();
                        sms_send_control_type = task.SelectSingleNode("sms-send-control-type") == null ? string.Empty : task.SelectSingleNode("sms-send-control-type").InnerText.Trim();
                        email_send_control_type = task.SelectSingleNode("email-send-control-type") == null ? string.Empty : task.SelectSingleNode("email-send-control-type").InnerText.Trim();
                    }
                    NewScheduler<T>(jobName, name, description, cron, child_path, business_type, weekend_push, send_control_type, sms_send_control_type, email_send_control_type, connection_name);
                }
            }
        }

        protected void NewScheduler<T>(string jobName, string name, string description, string cronExpression, string childpath, string business_type, string weekendPush,
            string send_control_type, string sms_send_control_type, string email_send_control_type, string connection_name) where T : IJob
        {
            var jobKey = new JobKey(jobName, name);
            _scheduler.DeleteJob(jobKey);
            IJobDetail job = JobBuilder.Create<T>().WithIdentity(jobKey).Build();
            job.JobDataMap.Put("ConnectionName", connection_name);
            job.JobDataMap.Put("TaskXMLPath", childpath);
            job.JobDataMap.Put("Name", name);
            job.JobDataMap.Put("Description", description);
            job.JobDataMap.Put("CronExpression", cronExpression);
            if (jobName == "CommonMsgPush")
            {
                job.JobDataMap.Put("BusinessType", business_type);
                job.JobDataMap.Put("WeekendPush", weekendPush);
                job.JobDataMap.Put("SendControlType", send_control_type);
                job.JobDataMap.Put("SMSSendControlType", sms_send_control_type);
                job.JobDataMap.Put("EmailSendControlType", email_send_control_type);
            }
            ITrigger trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cronExpression).Build();
            _scheduler.ScheduleJob(job, trigger);
        }

        protected override void OnStop()
        {
            _log.Info("停止调度任务。");
            _scheduler.Shutdown(true);
            SchedulerMetaData metaData = _scheduler.GetMetaData();
            _log.Info("执行了 " + metaData.NumberOfJobsExecuted + " 个任务。");
        }

        protected override void OnPause()
        {
            _log.Info("暂停调度任务。");
            _scheduler.PauseAll();
        }

        protected override void OnContinue()
        {
            _log.Info("恢复调度任务。");
            _scheduler.ResumeAll();
        }
    }
}

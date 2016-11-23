using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common.Logging;
using QuartzExtScheduler.Plugins;
using Quartz;
using Quartz.Impl;
using System.Xml;

namespace QuartzExtScheduler.Test
{
    public partial class MainForm : Form
    {
        private ILog _log = LogManager.GetCurrentClassLogger();
        private ISchedulerFactory _factory;
        private IScheduler _scheduler;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _factory = new StdSchedulerFactory();
            _scheduler = _factory.GetScheduler();
            _scheduler.Start();
        }

        private void btnDynamic_Click(object sender, EventArgs e)
        {
            //NewScheduler("0 46 11 * * ?");
            //NewScheduler("Certified_Developer_Login_No_Publish_Recruit", "0 0/1 * * * ?");
            //NewScheduler("0 45 11 * * ?");
            //NewScheduler("0 47 11 * * ?");
            //NewScheduler("0 53 11 * * ?");
            //NewScheduler("0 54 11 * * ?");
            try
            {
                GenerateTask<CommonMsgPush>("TaskMsgPushConfig//Summary.xml", "CommonMsgPush");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("通用调度程序异常，原因：{0}", ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateTask<CommonDataCleaner>("TaskDataCleanerConfig//Summary.xml", "CommonDataCleaner");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("通用调度程序异常，原因：{0}", ex.Message);
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
                        sms_send_control_type=task.SelectSingleNode("sms-send-control-type")==null?string.Empty:task.SelectSingleNode("sms-send-control-type").InnerText.Trim();
                        email_send_control_type = task.SelectSingleNode("email-send-control-type") == null ? string.Empty : task.SelectSingleNode("email-send-control-type").InnerText.Trim();
                    }
                    NewScheduler<T>(jobName, name, description, cron, child_path, business_type, weekend_push, send_control_type, sms_send_control_type, email_send_control_type, connection_name);
                }
            }
        }

        protected void NewScheduler<T>(string jobName, string name, string description, string cronExpression, string childpath, string business_type, string weekendPush, 
            string send_control_type, string sms_send_control_type,string email_send_control_type, string connection_name) where T : IJob
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
            //Trigger Start
            //ITrigger trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(CronExpression).Build();
            //immediately
            ITrigger trigger = TriggerBuilder.Create().WithIdentity(jobName, name).StartNow().Build();
            _scheduler.ScheduleJob(job, trigger);
        }

    }
}

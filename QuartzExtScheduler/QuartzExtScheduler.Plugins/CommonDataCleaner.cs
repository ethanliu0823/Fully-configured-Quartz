using Common.Logging;
using QuartzExtScheduler.Entity.CommonDataCleanerEntity;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using QuartzExtScheduler.Utility.Core;
using System.Xml.Linq;
using Newtonsoft.Json;
using QuartzExtScheduler.Utility.Data;
using System.Data;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace QuartzExtScheduler.Plugins
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class CommonDataCleaner : IJob
    {
        private string _connectionString = string.Empty;
        private ILog _log = LogManager.GetCurrentClassLogger();
        private IDictionary<string, string> _globalConfig = new Dictionary<string, string>();
        private SummaryTask _summaryTask = new SummaryTask();
        private Task _task = new Task();

        public void Execute(IJobExecutionContext context)
        {
            this._log.Info("开始执行通用数据清洗调度.");

            try
            {
                this._log.Info("初始化开始。");

                this.Init(context);

                this._log.Info(string.Format("初始化完毕,配置参数：{0}", JsonConvert.SerializeObject(this._summaryTask)));
            }
            catch (Exception ex)
            {
                this._log.Error(string.Format("初始化异常：{0}", ex));
            }

            if (string.IsNullOrWhiteSpace(this._connectionString))
            {
                this._log.Info("数据库连接字符串未配置，已结束。");
                return;
            }

            try
            {
                this._log.Info("解析xml开始。");

                this.AnalyzeTask();

                this._log.Info("解析xml完成。");
            }
            catch (Exception ex)
            {
                this._log.Error(string.Format("解析xml异常：{0}", ex));
            }

            try
            {
                this._log.Info(string.Format("执行脚本开始，name:{0}, description:{1}", this._task.Name, this._task.Description));

                Process();

                this._log.Info("执行脚本完成。");
            }
            catch (Exception ex)
            {
                this._log.Error(string.Format("执行脚本异常：{0}。", ex));
            }

            this._log.Info("结束执行通用数据清洗调度");
        }

        private void Process()
        {
            foreach (Item item in this._task.Items)
            {
                this._log.Info(string.Format("开始执行item,name={0} description={1}", item.Name, item.Description));

                DataSet ds = new DataSet();
                IList<string> defaultFields = new List<string>();
                IList<string> notNullFields = new List<string>();

                IDictionary<string, string> variables = this.CreateVariables(item.Variables);
                InitTarget(item.Target, variables, defaultFields, notNullFields);

                foreach (Selector selector in item.Selectors)
                {
                    this._log.Info(string.Format("开始执行selector,name={0} description={1}", selector.Name, selector.Description));

                    DataTable dt = null;
                    try
                    {

                        dt = this.ExecuteSelector(selector, variables);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("执行selector {0}出现异常", selector.Name), ex);
                    }

                    if (dt != null)
                    {
                        dt.TableName = selector.Name;
                        ds.Tables.Add(dt);
                    }

                    this._log.Info(string.Format("结束执行selector,name={0} description={1}", selector.Name, selector.Description));
                }

                ExecuteTarget(item.Target, ds, defaultFields, notNullFields);

                this._log.Info("结束执行item,name=" + item.Name + " description=" + item.Description);
            }
        }

        private void AnalyzeTask()
        {
            string taskXML = AppDomain.CurrentDomain.BaseDirectory + @"TaskDataCleanerConfig\" + this._summaryTask.ChildPath;
            XElement xml = XElement.Load(taskXML);

            XElement name = xml.Element("name");
            XElement description = xml.Element("description");
            if (name == null || string.IsNullOrWhiteSpace(name.Value))
            {
                throw new Exception("缺少 name 标记 或者 name 标记为空值。");
            }
            if (description == null || string.IsNullOrWhiteSpace(description.Value))
            {
                throw new Exception("缺少 description 标记 或者 description 标记为空值。");
            }

            this._task.Name = name.Value;
            this._task.Description = description.Value;
            this._task.Items = new List<Item>();

            XElement items = xml.Element("items");
            if (items == null)
            {
                throw new Exception("缺少 items 标记。");
            }
            if (!items.HasElements)
            {
                throw new Exception("缺少 item 标记。");
            }

            foreach (var item in items.Elements("item"))
            {
                XAttribute attrItemName = item.Attribute("name");
                XAttribute attrItemDescription = item.Attribute("description");
                if (attrItemName == null || string.IsNullOrWhiteSpace(attrItemName.Value))
                {
                    throw new Exception("标记 item 缺少 name 属性或者 name 属性为空值。");
                }
                if (attrItemDescription == null || string.IsNullOrWhiteSpace(attrItemDescription.Value))
                {
                    throw new Exception("标记 item 缺少 description 属性或者 description 属性为空值。");
                }

                Item obj = new Item();
                obj.Name = attrItemName.Value;
                obj.Description = attrItemDescription.Value;
                obj.Selectors = new List<Selector>();
                obj.Variables = new List<Var>();

                foreach (var v in item.Elements("var"))
                {
                    XElement eleVarName = v.Element("name");
                    XElement eleVarValue = v.Element("value");
                    XAttribute attrVarSource = v.Attribute("source");
                    if (eleVarName == null || string.IsNullOrWhiteSpace(eleVarName.Value))
                    {
                        throw new Exception("标记 var 缺少 name 标记或者 name 标记为空值。");
                    }
                    if (eleVarValue == null)
                    {
                        throw new Exception("标记 var 缺少 value 标记。");
                    }
                    if (attrVarSource == null || string.IsNullOrWhiteSpace(attrVarSource.Value))
                    {
                        throw new Exception("标记 var 缺少 source 属性或者 source 属性为空值。");
                    }
                    if ((attrVarSource.Value.ToLower() == "sql" || attrVarSource.Value.ToLower() == "system") &&
                        string.IsNullOrWhiteSpace(eleVarValue.Value))
                    {
                        throw new Exception("标记 var 的 value 标记为空值。");
                    }

                    Var vr = new Var();
                    vr.Name = v.Element("name").Value;
                    vr.Value = v.Element("value").Value;
                    vr.Source = this.GetSource(v.Attribute("source").Value);

                    obj.Variables.Add(vr);
                }

                if (item.Elements("selector").Count() < 1)
                {
                    throw new Exception("缺少 selector 标记。");
                }

                foreach (var s in item.Elements("selector"))
                {
                    XAttribute attrSelectorDayOfWeek = s.Attribute("dayOfWeek");
                    XAttribute attrSelectorName = s.Attribute("name");
                    XAttribute attrSelectorDescription = s.Attribute("description");
                    XAttribute attrSelectorType = s.Attribute("type");
                    if (attrSelectorDayOfWeek == null || string.IsNullOrWhiteSpace(attrSelectorDayOfWeek.Value))
                    {
                        throw new Exception("标记 selector 缺少 dayOfWeek 属性或者 dayOfWeek 属性为空值。");
                    }
                    if (attrSelectorName == null || string.IsNullOrWhiteSpace(attrSelectorName.Value))
                    {
                        throw new Exception("标记 selector 缺少 name 属性或者 name 属性为空值。");
                    }
                    if (attrSelectorDescription == null || string.IsNullOrWhiteSpace(attrSelectorDescription.Value))
                    {
                        throw new Exception("标记 selector 缺少 description 属性或者 description 属性为空值。");
                    }
                    if (attrSelectorType == null || string.IsNullOrWhiteSpace(attrSelectorType.Value))
                    {
                        throw new Exception("标记 selector 缺少 type 属性或者 type 属性为空值。");
                    }
                    if (string.IsNullOrWhiteSpace(s.Value))
                    {
                        throw new Exception("标记 selector 为空值。");
                    }

                    Selector slt = new Selector();
                    slt.DayOfWeek = attrSelectorDayOfWeek.Value;
                    slt.Name = attrSelectorName.Value;
                    slt.Description = attrSelectorDescription.Value;
                    slt.Type = this.GetSelectorType(attrSelectorType.Value);
                    slt.InnerSql = s.Value;

                    obj.Selectors.Add(slt);
                }

                XElement target = item.Element("target");
                if (target != null)
                {
                    XAttribute attrTargetTableName = target.Attribute("table-name");
                    if (attrTargetTableName == null || string.IsNullOrWhiteSpace(attrTargetTableName.Value))
                    {
                        throw new Exception("标记 target 缺少 属性 table-name 或者 table-name 为空值。");
                    }

                    obj.Target = new Target();
                    obj.Target.TableName = attrTargetTableName.Value;
                    obj.Target.AfterSql = target.Element("after-sql") == null ? null : target.Element("after-sql").Value;
                    obj.Target.BeforeSql = target.Element("before-sql") == null ? null : target.Element("before-sql").Value;
                }

                this._task.Items.Add(obj);
            }
        }

        private IDictionary<string, string> CreateVariables(IList<Var> list)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();

            foreach (Var v in list)
            {
                this._log.Info(string.Format("开始执行var:{0}", JsonConvert.SerializeObject(v)));

                string key = string.Empty;
                string value = string.Empty;

                switch (v.Source)
                {
                    case Source.Param:
                        key = v.Name;
                        value = v.Value;
                        break;
                    case Source.System:
                        if (!this._globalConfig.ContainsKey(v.Value))
                        {
                            throw new Exception("变量值在配置文件中未找到:" + JsonConvert.SerializeObject(v));
                        }
                        key = v.Name;
                        value = this._globalConfig[v.Value];
                        break;
                    case Source.Sql:
                        DataTable dt = null;
                        try
                        {
                            dt = MySQLHelper.ExecuteDataTable(this._connectionString, v.Value);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("变量{0}在执行sql时出现异常", v.Name), ex);
                        }

                        foreach (string n in v.Name.Split('|'))
                        {
                            key = n;
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                if (!dt.Columns.Contains(n))
                                {
                                    throw new Exception(string.Format("变量名{0}在查询结果中找不到,请检查参数名和查询字段名是否对应。", n));
                                }
                                value = dt.Rows[0].IsNull(n) ? null : dt.Rows[0][n].ToString();
                            }
                            else
                            {
                                value = null;
                            }
                        }
                        break;
                }

                if (!variables.ContainsKey(key))
                {
                    variables.Add(key, value);
                }
                else
                {
                    throw new Exception(string.Format("变量名{0}重复", key));
                }

                this._log.Info(string.Format("结束执行var:{0}", JsonConvert.SerializeObject(variables.Last())));
            }

            return variables;
        }

        private DataTable ExecuteSelector(Selector selector, IDictionary<string, string> variables)
        {
            string tempWeek = Convert.ToInt32(DateTime.Now.DayOfWeek).ToString();

            if (selector.DayOfWeek.ToLower() == "all" || selector.DayOfWeek.Split(',').Contains<string>(tempWeek))
            {
                string sql = this.ReplaceParams(selector.InnerSql, variables);

                if (selector.Type == SelectorType.DataTable)
                {
                    DataTable dt = MySQLHelper.ExecuteDataTable(this._connectionString, sql);

                    return dt;
                }
                else
                {
                    MySQLHelper.ExecuteNonQuery(this._connectionString, sql);

                    return null;
                }
            }

            return null;
        }

        private void InitTarget(Target target, IDictionary<string, string> variables, IList<string> defaultFields, IList<string> notNullFields)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.TableName))
            {
                return;
            }

            this._log.Info(string.Format("开始执行target,table-name={0}", target.TableName));

            if (!string.IsNullOrWhiteSpace(target.BeforeSql))
            {
                target.BeforeSql = this.ReplaceParams(target.BeforeSql, variables);
            }

            if (!string.IsNullOrWhiteSpace(target.AfterSql))
            {
                target.AfterSql = this.ReplaceParams(target.AfterSql, variables);
            }

            target.MainSql = this.CreateTargetMainSql(target.TableName, defaultFields, notNullFields);

            this._log.Info(string.Format("结束执行target,table-name={0}", target.TableName));
        }

        private string CreateTargetMainSql(string tableName, IList<string> defaultFields, IList<string> notNullFields)
        {
            string sql = string.Format("desc {0};", tableName);

            DataTable dt = MySQLHelper.ExecuteDataTable(this._connectionString, sql);

            string result = string.Empty;

            if (dt != null && dt.Rows.Count > 0)
            {
                StringBuilder fields = new StringBuilder();
                StringBuilder values = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    string field = Convert.ToString(row["Field"]);
                    string extra = Convert.ToString(row["Extra"]);
                    string def = Convert.ToString(row["Default"]);
                    string isNull = Convert.ToString(row["Null"]);

                    if (extra.ToLower() == "auto_increment")
                    {
                        continue;
                    }

                    fields.Append(field);
                    fields.Append(",");

                    if (!string.IsNullOrWhiteSpace(def))
                    {
                        defaultFields.Add(field);
                    }

                    if (isNull.ToLower() == "no")
                    {
                        notNullFields.Add(field);
                    }

                    values.AppendFormat("@{0}", field);
                    values.Append(",");
                }

                result = string.Format(" insert into {0} ({1}) values ({2}) ",
                    tableName, fields.ToString().Trim(','), values.ToString().Trim(','));
            }
            else
            {
                throw new Exception(string.Format("未找到表：{0}", tableName));
            }

            return result;
        }

        private void ExecuteTarget(Target target, DataSet ds, IList<string> defaultFields, IList<string> notNullFields)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.MainSql))
            {
                return;
            }

            if (ds == null || ds.Tables.Count < 1)
            {
                return;
            }

            MySqlConnection conn = null;
            MySqlTransaction transaction = null;
            MySqlCommand command = null;
            try
            {
                using (conn = new MySqlConnection(this._connectionString))
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (!string.IsNullOrWhiteSpace(target.BeforeSql))
                    {
                        try
                        {
                            command = new MySqlCommand(target.BeforeSql, conn, transaction);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("执行BeforeSql异常", ex);
                        }
                    }

                    foreach (DataTable dt in ds.Tables)
                    {
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            string sql = string.Empty;
                            foreach (DataRow row in dt.Rows)
                            {
                                sql = this.ReplaceParams(target.MainSql, row, dt.Columns, defaultFields, notNullFields, dt.TableName);
                                try
                                {
                                    command = new MySqlCommand(sql, conn, transaction);
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("执行MainSql异常", ex);
                                }

                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(target.AfterSql))
                    {
                        try
                        {
                            command = new MySqlCommand(target.AfterSql, conn, transaction);
                            command.CommandTimeout = 1000 * 60 * 10;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("执行AfterSql异常", ex);
                        }
                    }

                    transaction.Commit();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            //finally
            //{
            //    transaction.Dispose();
            //    command.Cancel();
            //    conn.Close();
            //    command.Dispose();
            //    conn.Dispose();
            //    transaction = null;
            //    command = null;
            //    conn = null;
            //}
        }

        private void Init(IJobExecutionContext context)
        {
            this._summaryTask.ConnectionName = context.JobDetail.JobDataMap["ConnectionName"].GetString();
            this._summaryTask.ChildPath = context.JobDetail.JobDataMap["TaskXMLPath"].GetString();
            this._summaryTask.Name = context.JobDetail.JobDataMap["Name"].GetString();
            this._summaryTask.Description = context.JobDetail.JobDataMap["Description"].GetString();
            this._summaryTask.CronExpression = context.JobDetail.JobDataMap["CronExpression"].GetString();

            List<string> allKey = ConfigurationManager.AppSettings.AllKeys.ToList();
            foreach (var key in allKey)
            {
                _globalConfig.Add(key.ToUpper(), ConfigurationManager.AppSettings[key]);
            }

            if (string.IsNullOrEmpty(this._summaryTask.ConnectionName))
            {
                this._summaryTask.ConnectionName = "Default";
            }
            this._connectionString = ConfigurationManager.ConnectionStrings[this._summaryTask.ConnectionName].GetString();
        }

        protected Source GetSource(string o)
        {
            Source result = Source.Param;

            switch (o.ToLower())
            {
                case "sql":
                    result = Source.Sql;
                    break;
                case "system":
                    result = Source.System;
                    break;
                case "param":
                    result = Source.Param;
                    break;
            }

            return result;
        }

        protected SelectorType GetSelectorType(string o)
        {
            SelectorType result = SelectorType.DataTable;

            switch (o.ToLower())
            {
                case "datatable":
                    result = SelectorType.DataTable;
                    break;
                case "nonquery":
                    result = SelectorType.NonQuery;
                    break;
            }

            return result;
        }

        protected string ReplaceParams(string str, IDictionary<string, string> variables)
        {
            Regex reg = new Regex(@"@\w+");
            foreach (Match match in reg.Matches(str))
            {
                string param = match.Value.Trim('@');
                if (!variables.ContainsKey(param))
                {
                    throw new Exception(string.Format("未找到参数：{0}", match.Value));
                }

                str = str.Replace(match.Value, variables[param] == null ? "null" : string.Format("'{0}'", variables[param]));
            }

            return str;
        }

        protected string ReplaceParams(string str, DataRow row, DataColumnCollection cols, IList<string> defaultFields, IList<string> notNullFields, string selectorName)
        {
            Regex reg = new Regex(@"@\w+");
            foreach (Match match in reg.Matches(str))
            {
                string param = match.Value.Trim('@');
                string temp = string.Empty;

                if (!cols.Contains(param))
                {
                    if (defaultFields.Contains(param))
                    {
                        temp = "default";
                    }
                    else if (notNullFields.Contains(param))
                    {
                        throw new Exception(string.Format("在selector {0}中未找到字段{1}，这是必填字段。", selectorName, param));
                    }
                    else
                    {
                        temp = "null";
                    }
                }
                else
                {
                    temp = row.IsNull(param) ? "null" : string.Format("'{0}'", row[param].ToString());
                }

                str = str.Replace(match.Value + ",", temp + ",").Replace(match.Value + ")", temp + ")");
            }

            return str;
        }
    }
}

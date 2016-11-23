using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QuartzExtScheduler.Utility.Data;
using MySql.Data.MySqlClient;

namespace QuartzExtScheduler.Common
{
    public class MessageTemplate
    {
        public static DataTable GetMessageTemplate(string conn, string templateId)
        {
            string sql = @"SELECT * FROM b2b_support.bizp_message_template WHERE message_template_id=@templateId";

            return MySQLHelper.ExecuteDataTable(conn, sql, new MySqlParameter("@templateId", templateId));
        }

        /// <summary>
        /// 根据模板id获取运营模板
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static DataTable GetOpMessageTemplate(string conn, string templateId)
        {
            string sql = @"SELECT * FROM dotnet_operation.op_message_template WHERE template_id=@templateId";

            return MySQLHelper.ExecuteDataTable(conn, sql, new MySqlParameter("@templateId", templateId));
        }

        /// <summary>
        /// 将模板中的变量替换为入参
        /// </summary>
        /// <param name="content"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ReplaceVariable(string content, Dictionary<string, string> parameters)
        {
            if (!string.IsNullOrEmpty(content) && parameters.Count > 0)
            {
                content = content.Replace("#换行#", @"\r\n");
                foreach (KeyValuePair<string, string> keyVal in parameters)
                {
                    string key = keyVal.Key;
                    string variable = "${" + key + "}";
                    string variable1 = "#{" + key + "}";
                    string variable2 = "#" + key + "#";
                    while (content.IndexOf(variable) > -1)
                    {
                        content = content.Replace(variable, parameters[key].Replace("#","号"));
                    }
                    while (content.IndexOf(variable1) > -1)
                    {
                        content = content.Replace(variable1, parameters[key].Replace("#", "号"));
                    }
                    while (content.IndexOf(variable2) > -1)
                    {
                        content = content.Replace(variable2, parameters[key].Replace("#", "号"));
                    }
                }
            }
            return content;
        }
    }
}

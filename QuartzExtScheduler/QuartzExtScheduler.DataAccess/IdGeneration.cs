using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QuartzExtScheduler.Utility.Core;
using QuartzExtScheduler.Utility.Data;
using MySql.Data.MySqlClient;

namespace QuartzExtScheduler.DataAccess
{
    /// <summary>
    /// ID生成
    /// </summary>
    public class IdGeneration
    {
        /// <summary>
        /// 获取ID根据指定的表名。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static long NewSequentialId(string connectionString, string dbName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            string commandText =
                @"SELECT `table_id`,`table_name` FROM b2b_parameter.`bsp_id_generation` WHERE `table_name` = @tableName OR `table_name` = @tableFullName FOR UPDATE";
            MySqlTransaction transaction = null;
            long id = 1;
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var command = new MySqlCommand(commandText, conn, transaction);
                    command.Parameters.Clear();
                    command.Parameters.AddRange(
                        new[]
                        {
                            new MySqlParameter("tableName", tableName),
                            new MySqlParameter("tableFullName",
                                string.IsNullOrEmpty(dbName)
                                    ? tableName
                                    : string.Format("{0}.{1}", dbName, tableName))
                        });
                    var dt = new DataTable();
                    var sda = new MySqlDataAdapter(command);
                    sda.Fill(dt);
                    var updateParames = new List<MySqlParameter>();
                    if (dt.Rows.Count > 0)
                    {
                        commandText =
                            "UPDATE b2b_parameter.`bsp_id_generation` SET `table_id` = @tableId WHERE `table_name` = @tableName";
                        id = Convert.ToInt64(dt.Rows[0]["table_id"]) + 1;
                        updateParames.Add(new MySqlParameter("tableName", dt.Rows[0]["table_name"].GetString()));
                    }
                    else
                    {
                        commandText =
                            "INSERT INTO b2b_parameter.`bsp_id_generation`(`table_name`,`table_id`) VALUES(@tableName,@tableId)";
                        updateParames.Add(new MySqlParameter("tableName",
                            string.IsNullOrEmpty(dbName) ? tableName : string.Format("{0}.{1}", dbName, tableName)));
                    }
                    updateParames.Add(new MySqlParameter("tableId", Convert.ToString(id)));
                    command = new MySqlCommand(commandText, conn, transaction);
                    command.Parameters.Clear();
                    command.Parameters.AddRange(updateParames.ToArray());
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    throw;
                }
            }
            return id;
        }

        /// <summary>
        /// 获取ID根据指定的表名。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static long NewSequentialId(string connectionString,string tableName)
        {
            return NewSequentialId(connectionString, null, tableName);
        }
    }
}

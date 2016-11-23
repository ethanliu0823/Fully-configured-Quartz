using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QuartzExtScheduler.Utility.Core;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace QuartzExtScheduler.Utility.Data
{
    public class MySQLHelper
    {
        public static readonly string DefaultConnection = ConfigurationManager.ConnectionStrings["Default"].GetString();

        /// <summary>
        /// 命令超时时间，默认60(秒)
        /// </summary>
        private static readonly int CommandTimeout = 600;

        public static int ExecuteNonQuery(string connectionString, int commandTimeout, string commandText,
            params MySqlParameter[] parameter)
        {
            return ExecuteNonQuery(connectionString, commandTimeout, commandText, CommandType.Text, parameter);
        }

        public static int ExecuteNonQuery(string connectionString, int commandTimeout, string commandText, CommandType commandType, params MySqlParameter[] parameter)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(commandText, conn)
                {
                    CommandTimeout = commandTimeout > CommandTimeout ? commandTimeout : CommandTimeout,
                    CommandType = commandType
                };
                PreparationParameters(cmd, parameter);
                return cmd.ExecuteNonQuery();
            }
        }

        public static int ExecuteNonQuery(string connectionString, string commandText,
            params MySqlParameter[] parameter)
        {
            return ExecuteNonQuery(connectionString, CommandTimeout, commandText, parameter);
        }

        public static object ExecuteScalar(string connectionString, int commandTimeout, string commandText,
            params MySqlParameter[] parameter)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(commandText, conn)
                {
                    CommandTimeout = commandTimeout > CommandTimeout ? commandTimeout : CommandTimeout
                };
                PreparationParameters(cmd, parameter);
                return cmd.ExecuteScalar();
            }
        }

        public static object ExecuteScalar(string connectionString, string commandText,
            params MySqlParameter[] parameter)
        {
            return ExecuteScalar(connectionString, CommandTimeout, commandText, parameter);
        }

        public static DataTable ExecuteDataTable(string connectionString, int commandTimeout, string commandText,
            params MySqlParameter[] parameter)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                var cmd = new MySqlCommand(commandText, conn)
                {
                    CommandTimeout = commandTimeout > CommandTimeout ? commandTimeout : CommandTimeout
                };
                PreparationParameters(cmd, parameter);
                var dt = new DataTable();
                var sda = new MySqlDataAdapter(cmd);
                sda.Fill(dt);
                return dt;
            }
            return null;
        }

        public static DataTable ExecuteDataTable(string connectionString, string commandText,
            params MySqlParameter[] parameter)
        {
            return ExecuteDataTable(connectionString, CommandTimeout, commandText, parameter);
        }

        protected static void PreparationParameters(MySqlCommand cmd, params MySqlParameter[] parameter)
        {
            if (cmd == null || parameter == null) return;
            cmd.Parameters.Clear();
            cmd.Parameters.AddRange(parameter);
        }
    }
}

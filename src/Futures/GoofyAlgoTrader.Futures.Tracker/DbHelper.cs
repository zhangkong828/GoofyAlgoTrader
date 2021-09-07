using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class DbHelper
    {
        public static DbConnection GetConnection()
        {
            string strConn = Config.GetValue("MySql:Connection", "");
            return GetConnection(strConn);
        }

        public static DbConnection GetConnection(string strConn)
        {
            return CreateConnection(DataBaseType.MySQL, strConn);
        }

        public static DbConnection CreateConnection(DataBaseType dbType, string strConn)
        {
            DbConnection connection = null;
            if (string.IsNullOrWhiteSpace(strConn))
                throw new ArgumentNullException("strConn");

            switch (dbType)
            {
                //case DataBaseType.SqlServer:
                //    connection = new SqlConnection(strConn);
                //    break;
                case DataBaseType.MySQL:
                    connection = new MySqlConnection(strConn);
                    break;
                //case DataBaseType.PostgreSQL:
                //    connection = new NpgsqlConnection(strConn);
                //    break;
                default:
                    throw new ArgumentNullException($"未支持的{dbType.ToString()}数据库类型");

            }
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

    }

    public enum DataBaseType
    {
        SqlServer,
        MySQL,
        PostgreSQL,
        SQLite,
    }
}

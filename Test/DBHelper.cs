using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class DBHelper
    {
        //连接字符串
        public static string connStr = "Server=.;database=RevitDB;uid=Lhj;pwd=Lhj990212";
        //连接对象
        public static SqlConnection conn = null;
        //定义数据适配器
        public static SqlDataAdapter adp = null;

        #region 连接数据库
        private static void OpenConn()
        {
            if (conn == null)
            {
                conn = new SqlConnection(connStr);
                conn.Open();
            }
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            if (conn.State == System.Data.ConnectionState.Broken)
            {
                conn.Close();
                conn.Open();
            }
        }
        #endregion

        #region 执行sql语句前的准备（将sql传入adapter对象）
        public static void PrepareSql(string sql)
        {
            OpenConn();
            adp = new SqlDataAdapter(sql, conn);
        }
        #endregion

        #region 设置sql语句参数
        public static void SetParameter(string parameterName, object parameterValue)
        {
            parameterName = "@" + parameterName.Trim();
            if (parameterValue == null)
            {
                parameterValue = DBNull.Value;
            }
            adp.SelectCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
        }
        #endregion

        #region 执行sql语句
        public static int ExecNonQuery()
        {
            int count = adp.SelectCommand.ExecuteNonQuery();
            conn.Close();
            return count;
        }

        public static DataTable ExecQuery()
        {
            DataTable dt = new DataTable();
            adp.Fill(dt);
            conn.Close();
            return dt;
        }

        public static SqlDataReader ExecDataReader()
        {
            return adp.SelectCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static object ExecScalar()
        {
            Object obj = adp.SelectCommand.ExecuteScalar();
            conn.Close();
            return obj;
        }

        #endregion
    }
}

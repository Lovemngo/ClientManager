using System;
using System.Data;
using System.Data.SqlClient;

namespace ClientManager.Data
{
    internal static class DbHelper
    {
        public static SqlParameter AddParam(SqlCommand cmd, string name, SqlDbType type, int size, object value)
        {
            SqlParameter p = size > 0
                ? cmd.Parameters.Add(name, type, size)
                : cmd.Parameters.Add(name, type);
            p.Value = value ?? DBNull.Value;
            return p;
        }

        public static string GetStringOrNull(IDataRecord r, int index)
        {
            return r.IsDBNull(index) ? null : r.GetString(index);
        }

        public static DateTime? GetDateOrNull(IDataRecord r, int index)
        {
            return r.IsDBNull(index) ? (DateTime?)null : r.GetDateTime(index);
        }
    }
}

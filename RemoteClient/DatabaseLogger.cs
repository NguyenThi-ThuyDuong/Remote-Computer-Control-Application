using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient
{
    public class DatabaseLogger
    {
        private readonly string cs;

        public DatabaseLogger(string connectionString)
        {
            cs = connectionString;
        }

        public void Log(string eventName, string detail)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO HeThongLog (SuKien, ChiTiet) VALUES (@s, @c)", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", eventName);
                        cmd.Parameters.AddWithValue("@c", detail);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Bạn có thể ghi log file fallback tại đây nếu muốn
            }
        }
    }
}

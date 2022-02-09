using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Helpers
{
    public class CacheLogger
    {
        public static void LogCache(string type, string key, object obj)
        {
            string payload = String.Empty;

            if (obj != null)
            {
                try
                {
                    payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
                catch { }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection("Server=DESKTOP-0CJAUCC;Database=FreshStore;Trusted_Connection=True;"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "LogCacheEntries";
                    //
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Key", key);
                    command.Parameters.AddWithValue("@Payload", payload);
                    //
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    //
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) { }

        }
    }
}

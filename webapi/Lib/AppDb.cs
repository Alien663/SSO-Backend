using System;
using System.Data.SqlClient;

namespace WebAPI.Lib
{
    public class AppDb : IDisposable
    {
        public SqlConnection Connection;
        public AppDb(string ConnectionStrings = null)
        {
            ConnectionStrings ??= AppConfig.Config["ConnectionStrings:Default"];
            Connection = new SqlConnection(ConnectionStrings);
        }
        public void Dispose()
        {

            Connection.Close();
            Connection.Dispose();
        }
    }
}

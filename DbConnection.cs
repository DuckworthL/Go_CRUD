using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace FoodOrderApp
{
    public class DbConnections
    {
        private static string strConnString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FoodOrderDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        
        public int ExecuteQuery(SqlCommand cmd)
        {
            using (SqlConnection connection = new SqlConnection(strConnString))
            {
                connection.Open();
                cmd.Connection = connection;
                int result = cmd.ExecuteNonQuery();
                return result;
            }
        }

        
        public object ExecuteScalar(SqlCommand cmd)
        {
            using (SqlConnection connection = new SqlConnection(strConnString))
            {
                connection.Open();
                cmd.Connection = connection;
                return cmd.ExecuteScalar();
            }
        }

        
        public void FillData(string query, DataTable table)
        {
            using (SqlConnection connection = new SqlConnection(strConnString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(table);
            }
        }

        
        public string GetConnString()
        {
            return strConnString;
        }
    }
}

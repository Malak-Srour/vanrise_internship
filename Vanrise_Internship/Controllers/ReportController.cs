using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Vanrise_Internship.Models;

namespace Vanrise_Internship.Controllers
{
    public class ReportsController : ApiController
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

        // Get the number of clients per type
        [HttpGet]
        [Route("api/Reports/GetClientsPerType")]
        public IHttpActionResult GetClientsPerType()
        {
            List<ClientReport> clientReports = new List<ClientReport>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Type, COUNT(*) as NoOfClients FROM Client GROUP BY Type";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clientReports.Add(new ClientReport
                    {
                        Type = (ClientType)Enum.Parse(typeof(ClientType), reader["Type"].ToString()),
                        NoOfClients = (int)reader["NoOfClients"]
                    });
                }
            }

            return Ok(clientReports);
        }
    }
}

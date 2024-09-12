using System.Collections.Generic;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;

namespace Vanrise_Internship.Controllers
{
    [RoutePrefix("devices")]
    public class DevicesReportController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

        [HttpGet]
        [Route("getDevices")]
        public IHttpActionResult GetDevices()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetDeviceReservationSummary", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                conn.Open();

                var devices = new List<DeviceDto>();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        devices.Add(new DeviceDto
                        {
                            DeviceName = reader["DeviceName"].ToString(),
                            Reserved = Convert.ToInt32(reader["Reserved"]),
                            Unreserved = Convert.ToInt32(reader["Unreserved"])
                        });
                    }
                }

                return Ok(devices);
            }
        }

        public class DeviceDto
        {
            public string DeviceName { get; set; }
            public int Reserved { get; set; }
            public int Unreserved { get; set; }

        }


    }
}
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
using Vanrise_Internship.Models;

public class DevicesController : ApiController
{
    private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

    [HttpGet]

    public IHttpActionResult GetAllDevices()
    {
        List<Device> devices = new List<Device>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "SELECT ID, Name FROM DeviceDb";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                devices.Add(new Device
                {
                    ID = (int)reader["ID"],
                    Name = reader["Name"].ToString()
                });
            }
        }

        return Ok(devices);
    }

    [HttpGet]
    [Route("api/Devices/GetFilteredDevices")]
    public IHttpActionResult GetFilteredDevices(string searchTerm)
    {
        List<Device> devices = new List<Device>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "SELECT ID, Name FROM DeviceDb WHERE Name LIKE @SearchTerm";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                devices.Add(new Device
                {
                    ID = (int)reader["ID"],
                    Name = reader["Name"].ToString()
                });
            }
        }

        return Ok(devices);
    }






    [HttpDelete]
    public IHttpActionResult DeleteDevice(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM DeviceDb WHERE ID = @ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
                return NotFound();
        }

        return Ok();
    }



    [HttpPost]
    public IHttpActionResult AddDevice(Device device)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO DeviceDb (Name) VALUES (@Name)";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", device.Name);
            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
                return BadRequest("No device was added.");

            return Ok();
        }
    }


    [HttpPut]
    [Route("api/Devices/UpdateDevice")]
    public IHttpActionResult UpdateDevice(Device device)
    {
        if (device == null)
        {
            return BadRequest("Device data is null");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "UPDATE DeviceDb SET Name = @Name WHERE ID = @ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", device.Name);
            cmd.Parameters.AddWithValue("@ID", device.ID);
            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return Ok();
        }
    }




}


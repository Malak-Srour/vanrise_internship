using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Vanrise_Internship.Models;

public class PhoneNumberController : ApiController
{
    private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

    [HttpGet]
    [Route("api/PhoneNumbers/GetAllPhoneNumbers")]
    public IHttpActionResult GetAllPhoneNumbers()
    {
        List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            SELECT pn.ID, pn.Number, pn.DeviceID, d.Name as DeviceName 
            FROM PhoneNumber pn
            INNER JOIN DeviceDb d ON pn.DeviceID = d.ID";  // Assuming Device table has ID and Name fields

            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                phoneNumbers.Add(new PhoneNumber
                {
                    ID = (int)reader["ID"],
                    Number = reader["Number"].ToString(),
                    DeviceID = (int)reader["DeviceID"],
                    DeviceName = reader["DeviceName"].ToString()  // Adding DeviceName here
                });
            }
        }

        return Ok(phoneNumbers);
    }



    [Route("api/PhoneNumbers/DeletePhoneNumber")]
    [HttpDelete]
    public IHttpActionResult DeletePhoneNumber(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM PhoneNumber WHERE ID = @ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", id);
            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
                return NotFound();
        }

        return Ok();
    }

    [Route("api/PhoneNumbers/AddPhoneNumber")]
    [HttpPost]
    public IHttpActionResult AddPhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber.Number.Length != 8 || !phoneNumber.Number.All(char.IsDigit))
        {
            return BadRequest("Phone number must be exactly 8 digits long and contain only numbers.");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            // Check if the phone number already exists
            string checkQuery = "SELECT COUNT(*) FROM PhoneNumber WHERE Number = @Number";
            SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@Number", phoneNumber.Number);
            conn.Open();  // Open connection here for the existence check

            int count = (int)checkCmd.ExecuteScalar();
            if (count > 0)
            {
                return BadRequest("This phone number is already in use.");
            }

            // Remove the explicit conn.Close(), the connection will remain open for the insert
            // Insert the new phone number if it doesn't exist
            string query = "INSERT INTO PhoneNumber (Number, DeviceID) VALUES (@Number, @DeviceID)";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Number", phoneNumber.Number);
            cmd.Parameters.AddWithValue("@DeviceID", phoneNumber.DeviceID);

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                return BadRequest("Failed to add phone number.");
            }

            return Ok();
        }
    }





    [HttpPut]
    [Route("api/PhoneNumbers/UpdatePhoneNumber")]
    public IHttpActionResult UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber == null)
        {
            return BadRequest("Phone number data is null");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            // Ensure you are only updating the necessary fields
            string query = "UPDATE PhoneNumber SET Number = @Number, DeviceID = @DeviceID WHERE ID = @ID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Number", phoneNumber.Number);
            cmd.Parameters.AddWithValue("@DeviceID", phoneNumber.DeviceID);
            cmd.Parameters.AddWithValue("@ID", phoneNumber.ID);
            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return Ok();
        }
    }





    [HttpGet]
    [Route("api/PhoneNumbers/SearchPhoneNumbers")]
    public IHttpActionResult SearchPhoneNumbers(string number = "", int? deviceId = null)
    {
        List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
        SELECT pn.ID, pn.Number, pn.DeviceID, d.Name as DeviceName 
        FROM PhoneNumber pn
        INNER JOIN DeviceDb d ON pn.DeviceID = d.ID
        WHERE 1=1";

            // If a number is provided, filter by the number
            if (!string.IsNullOrEmpty(number))
            {
                query += " AND pn.Number LIKE @Number";
            }

            // If a device ID is provided, filter by the device ID
            if (deviceId.HasValue)
            {
                query += " AND pn.DeviceID = @DeviceID";
            }

            SqlCommand cmd = new SqlCommand(query, conn);

            // Add parameters for filtering
            if (!string.IsNullOrEmpty(number))
            {
                cmd.Parameters.AddWithValue("@Number", "%" + number + "%");
            }
            if (deviceId.HasValue)
            {
                cmd.Parameters.AddWithValue("@DeviceID", deviceId.Value);
            }

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                phoneNumbers.Add(new PhoneNumber
                {
                    ID = (int)reader["ID"],
                    Number = reader["Number"].ToString(),
                    DeviceID = (int)reader["DeviceID"],
                    DeviceName = reader["DeviceName"].ToString()  // Add DeviceName here
                });
            }
        }

        return Ok(phoneNumbers);
    }




    [HttpGet]
    [Route("api/PhoneNumbers/SearchPhoneNumbersByDevice")]
    public IHttpActionResult SearchPhoneNumbersByDevice(int? deviceId)
    {
        if (!deviceId.HasValue)
        {
            return BadRequest("Device ID is required.");
        }

        List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            SELECT pn.ID, pn.Number, pn.DeviceID, d.Name as DeviceName 
            FROM PhoneNumber pn
            INNER JOIN DeviceDb d ON pn.DeviceID = d.ID
            WHERE pn.DeviceID = @DeviceID";  // Joining Device to get DeviceName

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DeviceID", deviceId.Value);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                phoneNumbers.Add(new PhoneNumber
                {
                    ID = (int)reader["ID"],
                    Number = reader["Number"].ToString(),
                    DeviceID = (int)reader["DeviceID"],
                    DeviceName = reader["DeviceName"].ToString()  // Adding DeviceName here
                });
            }
        }

        return Ok(phoneNumbers);
    }



    [HttpGet]
    [Route("api/PhoneNumbers/SearchPhoneNumbersByDevicePage")]
    public IHttpActionResult SearchPhoneNumbersByDevicePage(int? deviceId)
    {
        if (!deviceId.HasValue)
        {
            return BadRequest("Device ID is required.");
        }

        List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            SELECT pn.ID, pn.Number, pn.DeviceID, d.Name as DeviceName 
            FROM PhoneNumber pn
            INNER JOIN DeviceDb d ON pn.DeviceID = d.ID
            WHERE pn.DeviceID = @DeviceID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DeviceID", deviceId.Value);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                phoneNumbers.Add(new PhoneNumber
                {
                    ID = (int)reader["ID"],
                    Number = reader["Number"].ToString(),
                    DeviceID = (int)reader["DeviceID"],
                    DeviceName = reader["DeviceName"].ToString()
                });
            }
        }

        return Ok(phoneNumbers);
    }


}





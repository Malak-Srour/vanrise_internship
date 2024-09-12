using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Vanrise_Internship.Models;

namespace Vanrise_Internship.Controllers
{
    [RoutePrefix("clients")]
    public class ClientsController : ApiController
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

        // Get all clients
        [HttpGet]
        [Route("GetAllClients")]
        public IHttpActionResult GetAllClients()
        {
            List<Client> clients = new List<Client>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name, Type, BirthDate FROM Client";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        ID = (int)reader["ClientID"],
                        Name = reader["Name"].ToString(),
                        Type = (ClientType)Enum.Parse(typeof(ClientType), reader["Type"].ToString()),
                        BirthDate = reader["BirthDate"] as DateTime?
                    });
                }
            }

            return Ok(clients);
        }

        // Filter clients by name
        [HttpGet]
        [Route("GetFilteredClients")]
        public IHttpActionResult GetFilteredClients(string searchTerm)
        {
            List<Client> clients = new List<Client>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name, Type, BirthDate FROM Client WHERE Name LIKE @SearchTerm";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        ID = (int)reader["ClientID"],
                        Name = reader["Name"].ToString(),
                        Type = (ClientType)Enum.Parse(typeof(ClientType), reader["Type"].ToString()),
                        BirthDate = reader["BirthDate"] as DateTime?
                    });
                }
            }

            return Ok(clients);
        }

        // Filter clients by type
        [HttpGet]
        [Route("GetFilteredByType")]
        public IHttpActionResult GetFilteredByType(int clientType)
        {
            List<Client> clients = new List<Client>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ClientID, Name, Type, BirthDate FROM Client WHERE Type = @ClientType";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientType", clientType);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        ID = (int)reader["ClientID"],
                        Name = reader["Name"].ToString(),
                        Type = (ClientType)Enum.Parse(typeof(ClientType), reader["Type"].ToString()),
                        BirthDate = reader["BirthDate"] as DateTime?
                    });
                }
            }

            return Ok(clients);
        }



        // Add new client with server-side validation
        [HttpPost]
        [Route("AddClient")]
        public IHttpActionResult AddClient([FromBody] Client newClient)
        {
            if (newClient == null || string.IsNullOrEmpty(newClient.Name) || newClient.Type == 0)
            {
                return BadRequest("Invalid client data. Name and Type are required.");
            }

            // Age validation for Individual clients
            if (newClient.Type == ClientType.Individual && newClient.BirthDate.HasValue)
            {
                int age = CalculateAge(newClient.BirthDate.Value);
                if (age < 18)
                {
                    return BadRequest("Client must be at least 18 years old.");
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Check if the client name already exists
                string checkQuery = "SELECT COUNT(*) FROM Client WHERE Name = @Name";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Name", newClient.Name);
                conn.Open();
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    return BadRequest("A client with this name already exists.");
                }
                conn.Close();

                // If no duplicate, insert the new client
                string query = "INSERT INTO Client (Name, Type, BirthDate) VALUES (@Name, @Type, @BirthDate)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", newClient.Name);
                cmd.Parameters.AddWithValue("@Type", (int)newClient.Type);
                cmd.Parameters.AddWithValue("@BirthDate", newClient.BirthDate.HasValue ? (object)newClient.BirthDate.Value : DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }

        // Calculate age based on birthdate
        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }



        // Update existing client
        [HttpPut]
        [Route("UpdateClient")]
        public IHttpActionResult UpdateClient([FromBody] Client updatedClient)
        {
            if (updatedClient == null || string.IsNullOrEmpty(updatedClient.Name) || updatedClient.Type == 0)
            {
                return BadRequest("Invalid client data. Name and Type are required.");
            }

            // Age validation for Individual clients
            if (updatedClient.Type == ClientType.Individual && updatedClient.BirthDate.HasValue)
            {
                int age = CalculateAge(updatedClient.BirthDate.Value);
                if (age < 18)
                {
                    return BadRequest("Client must be at least 18 years old.");
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Client SET Name = @Name, Type = @Type, BirthDate = @BirthDate WHERE ClientID = @ClientID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", updatedClient.Name);
                cmd.Parameters.AddWithValue("@Type", (int)updatedClient.Type);
                cmd.Parameters.AddWithValue("@BirthDate", updatedClient.BirthDate.HasValue ? (object)updatedClient.BirthDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ClientID", updatedClient.ID);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return Ok();
        }



        // Delete a client by ID
        [HttpDelete]
        [Route("DeleteClient/{id}")]
        public IHttpActionResult DeleteClient(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Client WHERE ClientID = @ClientID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", id);
                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    return NotFound();  // Client not found
                }
            }

            return Ok();  // Client deleted successfully
        }




    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Vanrise_Internship.Models;

namespace Vanrise_Internship.Controllers
{
    [RoutePrefix("reservations")]
    public class PhoneNumberReservationController : ApiController
    {
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

        // Reserve Phone Number (POST)
        [HttpPost]
        [Route("ReservePhoneNumber")]
        public IHttpActionResult ReservePhoneNumber([FromBody] PhoneNumberReservation reservation)
        {
            // Log the received reservation data
            Console.WriteLine("Received reservation data: ClientID = " + reservation.ClientID + ", PhoneNumberID = " + reservation.PhoneNumberID);

            // Add more detailed logging here
            Console.WriteLine("ClientID: " + reservation.ClientID);
            Console.WriteLine("PhoneNumberID: " + reservation.PhoneNumberID);
            Console.WriteLine("BED: " + DateTime.Now);

            if (reservation == null || reservation.ClientID == 0 || reservation.PhoneNumberID == 0)
            {
                return BadRequest("Invalid reservation data");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO PhoneNumberReservation (ClientID, PhoneNumberID, BED, NULL) 
                         VALUES (@ClientID, @PhoneNumberID, @BED, NULL)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", reservation.ClientID);
                cmd.Parameters.AddWithValue("@PhoneNumberID", reservation.PhoneNumberID);
                cmd.Parameters.AddWithValue("@BED", DateTime.Now);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error: " + ex.Message);
                    return InternalServerError(ex); // Return a server error if the SQL fails
                }
            }

            return Ok();
        }





        // Check if phone number is already reserved
        [HttpGet]
        [Route("IsPhoneNumberReserved")]
        public IHttpActionResult IsPhoneNumberReserved(int phoneNumberID)
        {
            bool isReserved = false;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT COUNT(*) FROM PhoneNumberReservation WHERE PhoneNumberID = @PhoneNumberID AND EED IS NULL";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);
                conn.Open();

                int count = (int)cmd.ExecuteScalar();
                isReserved = count > 0;
            }

            return Ok(new { isReserved });
        }






        // Get all reservations
        [HttpGet]
        [Route("GetAllReservations")]
        public IHttpActionResult GetAllReservations()
        {
            List<dynamic> reservations = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Updated query with JOIN to get Client.Name and PhoneNumber.Number
                string query = @"
            SELECT r.ID, r.ClientID, c.Name AS ClientName, r.PhoneNumberID, p.Number AS PhoneNumber, r.BED, r.EED
            FROM PhoneNumberReservation r
            JOIN Client c ON r.ClientID = c.ClientID
            JOIN PhoneNumber p ON r.PhoneNumberID = p.ID";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(new
                    {
                        ID = (int)reader["ID"],
                        ClientID = (int)reader["ClientID"],
                        ClientName = reader["ClientName"].ToString(),
                        PhoneNumberID = (int)reader["PhoneNumberID"],
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        BED = (DateTime)reader["BED"],
                        EED = reader["EED"] as DateTime?
                    });
                }
            }

            return Ok(reservations);
        }

        // Unreserve Phone Number (POST)
        [HttpPost]
        [Route("UnreservePhoneNumber")]
        public IHttpActionResult UnreservePhoneNumber([FromBody] PhoneNumberReservation reservation)
        {
            if (reservation == null || reservation.ClientID == 0 || reservation.PhoneNumberID == 0)
            {
                return BadRequest("Invalid reservation data");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE PhoneNumberReservation 
                         SET EED = @EED 
                         WHERE ClientID = @ClientID 
                         AND PhoneNumberID = @PhoneNumberID 
                         AND EED IS NULL";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", reservation.ClientID);
                cmd.Parameters.AddWithValue("@PhoneNumberID", reservation.PhoneNumberID);
                cmd.Parameters.AddWithValue("@EED", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return BadRequest("No active reservation found for this phone number.");
                }
            }

            return Ok();
        }



        // Get reservations by PhoneNumberID
        [HttpGet]
        [Route("GetReservationsByPhoneNumber")]
        public IHttpActionResult GetReservationsByPhoneNumber(int phoneNumberID)
        {
            List<dynamic> reservations = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Query to filter by PhoneNumberID and get the names
                string query = @"
            SELECT r.ID, r.ClientID, c.Name AS ClientName, r.PhoneNumberID, p.Number AS PhoneNumber, r.BED, r.EED
            FROM PhoneNumberReservation r
            JOIN Client c ON r.ClientID = c.ClientID
            JOIN PhoneNumber p ON r.PhoneNumberID = p.ID
            WHERE r.PhoneNumberID = @PhoneNumberID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(new
                    {
                        ID = (int)reader["ID"],
                        ClientID = (int)reader["ClientID"],
                        ClientName = reader["ClientName"].ToString(),
                        PhoneNumberID = (int)reader["PhoneNumberID"],
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        BED = (DateTime)reader["BED"],
                        EED = reader["EED"] as DateTime?
                    });
                }
            }

            return Ok(reservations);
        }



        // Get reservations by ClientID
        [HttpGet]
        [Route("GetReservationsByClient")]
        public IHttpActionResult GetReservationsByClient(int clientID)
        {
            List<dynamic> reservations = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Query to filter by ClientID and get the reservation details
                string query = @"
            SELECT r.ID, r.ClientID, c.Name AS ClientName, r.PhoneNumberID, p.Number AS PhoneNumber, r.BED, r.EED
            FROM PhoneNumberReservation r
            JOIN Client c ON r.ClientID = c.ClientID
            JOIN PhoneNumber p ON r.PhoneNumberID = p.ID
            WHERE r.ClientID = @ClientID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientID);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(new
                    {
                        ID = (int)reader["ID"],
                        ClientID = (int)reader["ClientID"],
                        ClientName = reader["ClientName"].ToString(),
                        PhoneNumberID = (int)reader["PhoneNumberID"],
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        BED = (DateTime)reader["BED"],
                        EED = reader["EED"] as DateTime?
                    });
                }
            }

            return Ok(reservations);
        }



        // Get reserved phone numbers for a client
        [HttpGet]
        [Route("GetReservedPhoneNumbers")]
        public IHttpActionResult GetReservedPhoneNumbers(int clientId)
        {
            List<dynamic> reservedPhoneNumbers = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT r.PhoneNumberID, p.Number 
            FROM PhoneNumberReservation r
            JOIN PhoneNumber p ON r.PhoneNumberID = p.ID
            WHERE r.ClientID = @ClientID AND r.EED IS NULL";  // Active reservation

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClientID", clientId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservedPhoneNumbers.Add(new
                    {
                        ID = (int)reader["PhoneNumberID"],
                        Number = reader["Number"].ToString()
                    });
                }
            }

            return Ok(reservedPhoneNumbers);
        }





    }
}
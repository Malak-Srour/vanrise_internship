using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Vanrise_Internship.Models
{
    public class PhoneNumberReservation
    {
        public int ID { get; set; }                 // Unique identifier for the reservation
        public int ClientID { get; set; }           // Foreign Key for the Client
        public string ClientName { get; set; }      // Client Name (for display purposes)
        public int PhoneNumberID { get; set; }      // Foreign Key for the Phone Number
        public string PhoneNumber { get; set; }     // Phone Number (for display purposes)
        public DateTime BED { get; set; }           // Begin Effective Date
        public DateTime? EED { get; set; }          // End Effective Date (nullable)

        
    }
}

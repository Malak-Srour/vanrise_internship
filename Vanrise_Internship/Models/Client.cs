using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanrise_Internship.Models
{
    public class Client
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ClientType Type { get; set; }
        public DateTime? BirthDate { get; set; } // Nullable DateTime
    }

    public enum ClientType
    {
        Individual = 1,
        Organization = 2
    }


}
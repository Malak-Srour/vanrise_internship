using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vanrise_Internship.Models
{
    public class PhoneNumber
    {
        public int ID { get; set; }
        public string Number { get; set; }
        public int DeviceID { get; set; }
        public string DeviceName { get; set; }

    }
}

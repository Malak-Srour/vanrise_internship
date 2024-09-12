using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Vanrise_Internship.utilities
{
    public class DeviceMapper
    {
        public static List<T> MapDevices<T>(IDataReader reader, Func<IDataReader, T> mapFunction)
        {
            List<T> list = new List<T>();
            while (reader.Read())
            {
                list.Add(mapFunction(reader));
            }
            return list;
        }
    }

}
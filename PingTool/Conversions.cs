using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PingTool
{
    public static class Conversions
    {
        public static uint IPv4ToUInt32(IPAddress ipAddress)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            Array.Reverse(bytes);
            uint intAddress = BitConverter.ToUInt32(bytes, 0);

            return intAddress;
        }

        public static IPAddress UInt32ToIPv4(uint intIp)
        {
            byte[] bytes = BitConverter.GetBytes(intIp);
            Array.Reverse(bytes); // flip little-endian to big-endian(network order)
            return new IPAddress(bytes);
        }

        //public static int IPv4ToInt32(IPAddress ipAddress)
        //{
        //    byte[] bytes = ipAddress.GetAddressBytes();
        //    int intAddress = BitConverter.ToInt32(bytes, 0);

        //    return intAddress;
        //}

        //public static IPAddress Int32ToIPv4(int intIp)
        //{
        //    byte[] bytes = BitConverter.GetBytes(intIp);
        //    return new IPAddress(bytes);
        //}
    }
}

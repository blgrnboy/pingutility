using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace PingTool
{
    public class NodePingReply
    {
        /// <summary>
        /// The date when the reply was received
        /// </summary>
        public DateTime ReplyDate { get; internal set; }
        
        // Store IP Address as int
        /// <summary>
        /// IP Address which replied to the ping
        /// </summary>
        public uint IPAddress { get; internal set; }

        /// <summary>
        /// Reply time in ms.
        /// </summary>
        public long ReplyTime { get; internal set; }

        /// <summary>
        /// Status of the reply
        /// </summary>
        public IPStatus Status { get; internal set; }
    }
}

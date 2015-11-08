using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PingTool
{
    public class Node
    {
        public Node(string nameOrIp, HostValidator validator)
        {
            IPAddresses = new List<uint>();
            PingReplies = new List<NodePingReply>();
            Name = nameOrIp;
            validator.Nodes.Add(this);
        }

        /// <summary>
        /// The name of the Node.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The IP Address of the node.
        /// </summary>
        public List<uint> IPAddresses { get; internal set; }

        /// <summary>
        /// Collection of all the ping replies for this node.
        /// </summary>
        public List<NodePingReply> PingReplies { get; internal set; }

        /// <summary>
        /// The successful ping reply with the lowest response time.
        /// </summary>
        public NodePingReply FastestSuccessfulReply { get; internal set; }

        /// <summary>
        /// The successful ping reply with the highest reponse time.
        /// </summary>
        public NodePingReply SlowestSuccessfulReply { get; internal set; }

        /// <summary>
        /// Last failed ping reply.
        /// </summary>
        public NodePingReply LastFailedReply { get; internal set; }

        /// <summary>
        /// Indicates whether the host is a valid IP address or DNS resolvable hostname
        /// </summary>
        public bool IsValidHost
        {
            get { return _isValidHost; }
            internal set
            {
                _isValidHost = value;
            }
        }

        private bool _isValidHost { get; set; }
    }
}

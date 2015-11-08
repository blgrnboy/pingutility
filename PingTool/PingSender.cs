using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace PingTool
{
    internal class PingSender
    {
        List<Node> hostsToPing;

        internal PingSender(List<Node> nodes)
        {
            hostsToPing = nodes;
        }

        internal void SendPings()
        {
            List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();

            foreach (var node in hostsToPing)
            {
                pingTasks.Add(PingAsync(Conversions.UInt32ToIPv4(node.IPAddresses[0])));
            }

            Task.WaitAll(pingTasks.ToArray());

            for (int i = 0; i < pingTasks.Count; i++)
            {
                var pingReply = pingTasks[i].Result;

                if (pingReply != null && pingReply.Status == IPStatus.Success)
                {
                    var reply =
                        new NodePingReply()
                        {
                            IPAddress = Conversions.IPv4ToUInt32(pingReply.Address),
                            ReplyDate = DateTime.UtcNow,
                            Status = pingReply.Status,
                            ReplyTime = pingReply.RoundtripTime
                        };

                    // Add to replies
                    hostsToPing[i].PingReplies.Add(reply);

                    // Check if faster or slowest reply time
                    if (hostsToPing[i].SlowestSuccessfulReply != null)
                    {
                        if (pingReply.RoundtripTime > hostsToPing[i].SlowestSuccessfulReply.ReplyTime)
                        {
                            hostsToPing[i].SlowestSuccessfulReply = reply;
                        }
                    }
                    else
                    {
                        hostsToPing[i].SlowestSuccessfulReply = reply;
                    }

                    if (hostsToPing[i].FastestSuccessfulReply != null)
                    {
                        if (pingReply.RoundtripTime < hostsToPing[i].FastestSuccessfulReply.ReplyTime)
                        {
                            hostsToPing[i].FastestSuccessfulReply = reply;
                        }
                    }
                    else
                    {
                        hostsToPing[i].FastestSuccessfulReply = reply;
                    }

                }
                else if (pingReply != null)
                {
                    var reply =
                        new NodePingReply()
                        {
                            IPAddress = Conversions.IPv4ToUInt32(pingReply.Address),
                            ReplyDate = DateTime.UtcNow,
                            Status = pingReply.Status,
                            ReplyTime = pingReply.RoundtripTime
                        };

                    hostsToPing[i].PingReplies.Add(reply);
                    hostsToPing[i].LastFailedReply = reply;
                }
                else
                {
                    var reply =
                            new NodePingReply()
                            {
                                IPAddress = hostsToPing[i].IPAddresses[0],
                                ReplyDate = DateTime.UtcNow,
                                Status = IPStatus.Unknown,
                                ReplyTime = 0
                            };

                    hostsToPing[i].PingReplies.Add(reply);
                    hostsToPing[i].LastFailedReply = reply;
                }
            }
        }

        private Task<PingReply> PingAsync(IPAddress ipAddress)
        {
            var tcs = new TaskCompletionSource<PingReply>();
            Ping ping = new Ping();
            ping.PingCompleted += (obj, sender) =>
            {
                tcs.SetResult(sender.Reply);
            };
            ping.SendAsync(ipAddress, new object());
            return tcs.Task;
        }
    }
}

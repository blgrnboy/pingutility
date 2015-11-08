using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PingTool
{
    public class HostValidator
    {
        private List<Node> _validNodes;
        private List<Node> _invalidNodes;
        private Timer validationTimer;
        private Timer pingTimer;

        private PingSender pingSender;

        public HostValidator()
        {
            Nodes = new ObservableCollection<Node>();
            _validNodes = new List<Node>();
            _invalidNodes = new List<Node>();
            CreateDataTable();

            validationTimer = new Timer();
            validationTimer.AutoReset = true;
            validationTimer.Interval = 60000;
            validationTimer.Elapsed += ValidationTimer_Elapsed;
            validationTimer.Enabled = true;

            pingTimer = new Timer();
            pingTimer.AutoReset = true;
            pingTimer.Interval = 3000;
            pingTimer.Elapsed += PingTimer_Elapsed; ;
            pingTimer.Enabled = true;

            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        private void CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("IP Address", typeof(string));
            table.Columns.Add("Status", typeof(IPStatus));
            table.Columns.Add("Last RTT", typeof(long));
            table.Columns.Add("Min RTT", typeof(long));
            table.Columns.Add("Max RTT", typeof(long));

            DataTable = table;
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (pingSender == null)
            {
                pingSender = new PingSender(_validNodes);
            }

            pingSender.SendPings();

            foreach (var node in _validNodes)
            {
                var lastPingReply = node.PingReplies.LastOrDefault();

                if (lastPingReply != null)
                {
                    bool isNodeListed = false;

                    lock (DataTable)
                    {
                        var row = DataTable.Rows.OfType<DataRow>().Where(x => DBNull.Value != x["Name"] && x["Name"] == node.Name).FirstOrDefault();

                        if (row != null)
                        {
                            isNodeListed = true;
                        }
                        else
                        {
                            row = DataTable.NewRow();
                        }
                        
                        row["Name"] = node.Name;
                        row["IP Address"] = Conversions.UInt32ToIPv4(lastPingReply.IPAddress);
                        row["Status"] = lastPingReply.Status;
                        row["Last RTT"] = lastPingReply.ReplyTime;

                        if (node.FastestSuccessfulReply != null && node.SlowestSuccessfulReply != null)
                        {
                            row["Min RTT"] = node.FastestSuccessfulReply.ReplyTime;
                            row["Max RTT"] = node.SlowestSuccessfulReply.ReplyTime;
                        }
                        else
                        {
                            row["Min RTT"] = DBNull.Value;
                            row["Max RTT"] = DBNull.Value;
                        }

                        if (!isNodeListed)
                        {
                            DataTable.Rows.Add(row);
                        }
                    }
                    
                }

            }
        }

        private void ValidationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Valid Nodes: " + NumValidNodes + "\tInvalidNodes: " + NumInvalidNodes);

            foreach (var node in _invalidNodes)
            {
                Validate(node);
            }
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    Validate((Node)item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (((Node)item).IsValidHost)
                    {
                        _validNodes.Remove((Node)item);
                    }
                    else
                    {
                        _invalidNodes.Remove((Node)item);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var node in Nodes)
                {
                    Validate(node);
                }
                _validNodes = Nodes.Where(x => x.IsValidHost == true).ToList();
                _invalidNodes = Nodes.Where(x => x.IsValidHost == false).ToList();
            }
        }

        public int NumInvalidNodes { get { return _invalidNodes.Count; } }

        public int NumValidNodes { get { return _validNodes.Count; } }

        public ObservableCollection<Node> Nodes { get; internal set; }

        public DataTable DataTable { get; private set; }

        private void Validate(Node node)
        {
            IPAddress ipAddress;

            if (IPAddress.TryParse(node.Name, out ipAddress))
            {
                node.IPAddresses.Add(Conversions.IPv4ToUInt32(ipAddress));
                node.IsValidHost = true;
            }

            if (ipAddress != null)
            {
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
                    node.Name = hostEntry.HostName;
                    foreach (var address in hostEntry.AddressList)
                    {
                        node.IPAddresses.Add(Conversions.IPv4ToUInt32(address));
                        node.IsValidHost = true;
                    }

                    Console.WriteLine(hostEntry.HostName);
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    Trace.WriteLine("Exception occured while trying to resolve DNS for \"" + ipAddress.ToString() + "\"");
                    Trace.WriteLine(ex.Message);
                    node.IsValidHost = true;
                    node.Name = ipAddress.ToString();
                    node.IPAddresses.Add(Conversions.IPv4ToUInt32(ipAddress));
                }

                _validNodes.Add(node);
            }
            else
            {
                var tempName = node.Name;

                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(tempName);
                    node.Name = hostEntry.HostName;
                    foreach (var address in hostEntry.AddressList)
                    {
                        node.IPAddresses.Add(Conversions.IPv4ToUInt32(address));
                        node.IsValidHost = true;
                        _validNodes.Add(node);
                    }
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    Trace.WriteLine("Exception occured while trying to resolve DNS for \"" + tempName + "\"");
                    Trace.WriteLine(ex.Message);
                    node.IsValidHost = false;
                    _invalidNodes.Add(node);
                }
            }
        }
    }
}

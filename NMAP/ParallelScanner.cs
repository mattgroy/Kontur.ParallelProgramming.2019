using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
    public class ParallelScanner : IPScanner
    {
        protected virtual ILog log => LogManager.GetLogger(typeof(SequentialScanner));

        public Task Scan(IPAddress[] ipAddrs, int[] ports)
        {
            var tasks = ipAddrs
                .Select(ipAddr =>
                    PingAddr(ipAddr)
                        .ContinueWith(prevTask =>
                        {
                            if (prevTask.Result != IPStatus.Success) return;
                            foreach (var port in ports) CheckPort(ipAddr, port);
                        }));

            return Task.WhenAll(tasks);
        }

        private Task<IPStatus> PingAddr(IPAddress ipAddr, int timeout = 3000)
        {
            log.Info($"Pinging {ipAddr}");
            var ping = new Ping();
            return ping.SendPingAsync(ipAddr, timeout).ContinueWith(prevTask =>
            {
                ping.Dispose();
                var status = prevTask.Result.Status;
                log.Info($"Pinged {ipAddr}: {status}");
                return status;
            });
        }

        private Task CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
        {
            var tcpClient = new TcpClient();
            return tcpClient.ConnectAsync(ipAddr, port, timeout)
                .ContinueWith(prevTask =>
                {
                    tcpClient.Close();
                    PortStatus portStatus;
                    switch (prevTask.Result.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            portStatus = PortStatus.OPEN;
                            break;
                        case TaskStatus.Faulted:
                            portStatus = PortStatus.CLOSED;
                            break;
                        default:
                            portStatus = PortStatus.FILTERED;
                            break;
                    }

                    log.Info($"Checked {ipAddr}:{port} - {portStatus}");
                }, TaskContinuationOptions.AttachedToParent);
        }
    }
}
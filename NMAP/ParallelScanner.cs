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
        private ILog log => LogManager.GetLogger(typeof(SequentialScanner));

        public Task Scan(IPAddress[] ipAddrs, int[] ports)
        {
            var tasks = ipAddrs.Select(async ipAddr =>
            {
                if (await PingAddr(ipAddr) != IPStatus.Success) return;
                await Task.WhenAll(ports.Select(port => CheckPort(ipAddr, port)));
            });
            
            return Task.WhenAll(tasks);
        }

        private async Task<IPStatus> PingAddr(IPAddress ipAddr, int timeout = 3000)
        {            
            log.Info($"Pinging {ipAddr}");
            using (var ping = new Ping())
            {
                var status = (await ping.SendPingAsync(ipAddr, timeout)).Status;
                log.Info($"Pinged {ipAddr}: {status}");
                return status;
            }
        }

        private async Task CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
        {
            using (var tcpClient = new TcpClient())
            {
                var res = await tcpClient.ConnectAsync(ipAddr, port, timeout);
                PortStatus portStatus;
                switch (res.Status)
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
            }
        }
    }
}
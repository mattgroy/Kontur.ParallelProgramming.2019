using System;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Utils;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses)
            : base(replicaAddresses)
        {
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = this.ReplicaAddresses.Select(address => SendRequestAsync(address, query));
            return await Task.WhenAny(tasks).WithTimeoutAsync(timeout);
        }
    }
}
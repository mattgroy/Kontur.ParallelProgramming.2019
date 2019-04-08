using System;
using System.Threading.Tasks;
using ClusterClient.Utils;
using log4net;

namespace ClusterClient.Clients
{
    public class RandomClusterClient : ClusterClientBase
    {
        private readonly Random random = new Random();

        public RandomClusterClient(string[] replicaAddresses)
            : base(replicaAddresses)
        {
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uri = this.ReplicaAddresses[this.random.Next(this.ReplicaAddresses.Length)];
//            return await SendRequestWithTimeoutAsync(uri, query, timeout);
            return await SendRequestAsync(uri, query).WithTimeoutAsync(timeout);
        }
    }
}
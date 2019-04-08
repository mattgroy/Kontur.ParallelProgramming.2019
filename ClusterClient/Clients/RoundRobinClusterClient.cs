using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ClusterClient.Utils;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        private readonly ConcurrentDictionary<string, TimeSpan> replicasTimeStats =
            new ConcurrentDictionary<string, TimeSpan>();

        public RoundRobinClusterClient(string[] replicaAddresses)
            : base(replicaAddresses)
        {
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            timeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / this.ReplicaAddresses.Length);

//            var shuffledReplicas = this.replicasTimeStats.Count == this.ReplicaAddresses.Length 
//                ? this.ReplicaAddresses.OrderBy(uri => this.replicasTimeStats[uri]) 
//                : this.ReplicaAddresses.Shuffle();
//            
//            foreach (var uri in shuffledReplicas)
//            {
//                var sw = Stopwatch.StartNew();
//                try
//                {
//                    var result = await SendRequestWithTimeoutAsync(uri, query, timeout);
//                    this.replicasTimeStats[uri] = sw.Elapsed;
//                    return result;
//                }
//                catch (TimeoutException) { }
//            }
//            throw new TimeoutException();

            var shuffledReplicas = this.ReplicaAddresses.Shuffle();

            foreach (var uri in shuffledReplicas)
                try
                {
                    return await SendRequestAsync(uri, query).WithTimeoutAsync(timeout);
                }
                catch (TimeoutException)
                {
                }

            throw new TimeoutException();
        }
    }
}
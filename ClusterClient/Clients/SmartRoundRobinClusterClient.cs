using System;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Utils;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartRoundRobinClusterClient : ClusterClientBase
    {
        public SmartRoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartRoundRobinClusterClient));

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var timeoutDelta = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / this.ReplicaAddresses.Length);
            var count = 0;

//            var tasks = new List<Task>();
//            foreach (var uri in this.ReplicaAddresses.Shuffle())
//            {
//                tasks.Add(SendRequestAsync(uri, query));
//                var timeoutTask = Task.Delay(timeout);
//                var resultTask = await tasks.With(timeoutTask).WaitForSuccess();
//
//                if (resultTask is Task<string> requestTask)
//                    return requestTask.Result;
//            }
//            throw new TimeoutException();

//            var tasks = this.ReplicaAddresses
//                .Shuffle()
//                .Select(address =>
//                    Task.Delay(timeoutDelta.Milliseconds * count++)
//                        .ContinueWith(prev => SendRequestAsync(address, query)).Result);
//            return await Task.WhenAny(tasks).WithTimeoutAsync(timeout);

            var tasks = this.ReplicaAddresses.Select(address => SendRequestAsync(address, query));
            return await Task.WhenAny(tasks).WithTimeoutAsync(timeout);
        }
    }
}
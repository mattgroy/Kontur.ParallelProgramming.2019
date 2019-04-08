using ClusterClient.Clients;
using NUnit.Framework;

namespace ClusterTests
{
    public class StupidClusterClientTest : ClusterTest
    {
        protected override ClusterClientBase CreateClient(string[] replicaAddresses)
        {
            return new ParallelClusterClient(replicaAddresses);
        }

        private const int Fast = 1000;

        [Test]
        public void ClientShouldReturnSuccess_WhenOneReplicaIsGoodAndOthersAreBad()
        {
            CreateServer(Fast);
            CreateServer(Fast, true);
            for (var i = 0; i < 3; i++)
                CreateServer(Slow);

            ProcessRequests(Timeout);
        }

        [Test]
        public void ClientShouldReturnSuccess_WhenTimeoutIsClose()
        {
            for (var i = 0; i < 4; i++)
                CreateServer(Fast);

            ProcessRequests(Fast + 100);
        }
    }
}
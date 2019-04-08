using ClusterClient.Clients;

namespace ClusterTests
{
    public class RandomClusterClientTest : ClusterTest
    {
        protected override ClusterClientBase CreateClient(string[] replicaAddresses)
        {
            return new RandomClusterClient(replicaAddresses);
        }
    }
}
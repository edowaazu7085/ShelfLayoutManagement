using Grpc.Net.Client;

namespace ShelfLayoutManagement.Tests.Integration
{
    public class IntegrationTestBase : IDisposable
    {
        private GrpcChannel? _channel;
        private IDisposable? _testContext;

        protected GrpcChannel Channel => _channel ??= CreateChannel();

        protected GrpcChannel CreateChannel()
        {
            return GrpcChannel.ForAddress("https://localhost:5001");
        }

        public void Dispose()
        {
            _testContext?.Dispose();
            _channel = null;
        }
    }
}

using Redis.OM;
using redis_test.Models;

namespace redis_test.HostedServices
{
    public class IndexCreationService : IHostedService
    {
        private readonly RedisConnectionProvider _provider;
        public IndexCreationService(RedisConnectionProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _provider.Connection.CreateIndexAsync(typeof(Person));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

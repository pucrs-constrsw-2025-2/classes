using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace ClasseMicroservice.Tests.Integration.External
{
    public class MongoContainerFixture : IAsyncLifetime
    {
        private readonly IContainer _container;
        public string ConnectionString { get; private set; } = string.Empty;
        public string DatabaseName { get; private set; } = $"classes_test_{Guid.NewGuid():N}";

        public MongoContainerFixture()
        {
            _container = new ContainerBuilder()
                .WithImage("mongo:7")
                .WithName($"classes-mongo-{Guid.NewGuid():N}")
                .WithPortBinding(0, 27017)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            var host = _container.Hostname;
            var port = _container.GetMappedPublicPort(27017);
            ConnectionString = $"mongodb://{host}:{port}";
        }

        public async Task DisposeAsync()
        {
            try { await _container.StopAsync(); } catch { /* ignore */ }
            try { await _container.DisposeAsync(); } catch { /* ignore */ }
        }
    }
}

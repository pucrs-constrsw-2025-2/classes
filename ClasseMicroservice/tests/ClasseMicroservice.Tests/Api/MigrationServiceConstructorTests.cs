using System.Collections.Generic;
using ClasseMicroservice.API.Data;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClasseMicroservice.Tests.Api
{
    public class MigrationServiceConstructorTests
    {
        [Fact]
        public void MigrationService_Should_Construct_With_Config()
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDb"] = "mongodb://localhost:27017/classes-test",
                ["MongoDbSettings:DatabaseName"] = "classes-test"
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();

            var svc = new MigrationService(cfg);
            svc.Should().NotBeNull();
        }
    }
}

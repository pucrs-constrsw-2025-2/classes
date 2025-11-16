using System.Collections.Generic;
using ClasseMicroservice.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClasseMicroservice.Tests.Infrastructure
{
    public class RepositoriesConstructorTests
    {
        private static IConfiguration BuildConfig()
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDb"] = "mongodb://localhost:27017/classes-test",
                ["MongoDbSettings:DatabaseName"] = "classes-test",
                ["MongoDbSettings:CollectionName"] = "Classes"
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        [Fact]
        public void ClassRepository_Should_Construct_With_Config()
        {
            var cfg = BuildConfig();
            var repo = new ClassRepository(cfg);
            repo.Should().NotBeNull();
        }

        [Fact]
        public void ExamRepository_Should_Construct_With_Config()
        {
            var cfg = BuildConfig();
            var repo = new ExamRepository(cfg);
            repo.Should().NotBeNull();
        }
    }
}

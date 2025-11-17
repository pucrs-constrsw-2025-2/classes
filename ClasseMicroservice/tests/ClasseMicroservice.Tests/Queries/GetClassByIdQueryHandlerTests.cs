using System.Threading.Tasks;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Queries
{
    public class GetClassByIdQueryHandlerTests
    {
        [Fact]
        public async Task ReturnsEntityFromRepository()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            var expected = new Class { Id = "ID-1" };
            repo.Setup(r => r.GetByIdAsync("ID-1")).ReturnsAsync(expected);

            var handler = new GetClassByIdQueryHandler(repo.Object);
            var result = await handler.HandleAsync(new GetClassByIdQuery("ID-1"));

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task ReturnsNull_WhenNotFound()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetByIdAsync("ID-404")).ReturnsAsync((Class?)null);

            var handler = new GetClassByIdQueryHandler(repo.Object);
            var result = await handler.HandleAsync(new GetClassByIdQuery("ID-404"));

            result.Should().BeNull();
        }
    }
}

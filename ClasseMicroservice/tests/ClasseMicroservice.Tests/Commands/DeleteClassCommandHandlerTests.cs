using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Interfaces;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands
{
    public class DeleteClassCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_CallsDeleteAsync()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            repo.Setup(r => r.DeleteAsync("ID-1")).Returns(Task.CompletedTask).Verifiable();

            var handler = new DeleteClassCommandHandler(repo.Object);
            await handler.HandleAsync(new DeleteClassCommand("ID-1"));

            repo.Verify();
        }
    }
}

using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands
{
    public class UpdateClassCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_CallsRepositoryWithIdAndEntity()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            var entity = new Class { Id = "ID-1", ClassNumber = "01", Year = 2025, Semester = 2, Schedule = "Mon" };
            repo.Setup(r => r.UpdateAsync("ID-1", entity)).Returns(Task.CompletedTask).Verifiable();

            var handler = new UpdateClassCommandHandler(repo.Object);
            await handler.HandleAsync(new UpdateClassCommand("ID-1", entity));

            repo.Verify();
        }
    }
}

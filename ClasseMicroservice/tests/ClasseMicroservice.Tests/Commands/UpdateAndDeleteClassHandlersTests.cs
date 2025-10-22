using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands;

public class UpdateAndDeleteClassHandlersTests
{
    [Fact]
    public async Task Update_Handler_CallsRepositoryWithIdAndEntity()
    {
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Class>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateClassCommandHandler(repo.Object);
        var cls = new Class { Id = "id-1", ClassNumber = "101" };
        var cmd = new UpdateClassCommand("id-1", cls);

        await handler.HandleAsync(cmd);

        repo.Verify(r => r.UpdateAsync("id-1", cls), Times.Once);
    }

    [Fact]
    public async Task Delete_Handler_CallsRepositoryWithId()
    {
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new DeleteClassCommandHandler(repo.Object);
        var cmd = new DeleteClassCommand("id-2");

        await handler.HandleAsync(cmd);

        repo.Verify(r => r.DeleteAsync("id-2"), Times.Once);
    }
}

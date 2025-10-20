using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands;

public class AddExamCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AssignsId_WhenMissing_AndCallsRepository()
    {
        var repo = new Mock<IExamRepository>();
        repo.Setup(r => r.AddExamToClassAsync(It.IsAny<string>(), It.IsAny<Exam>()))
            .Returns(Task.CompletedTask);

        var handler = new AddExamCommandHandler(repo.Object);
        var exam = new Exam { Id = null, Name = "P1" };
        var cmd = new AddExamCommand("class-1", exam);

        await handler.HandleAsync(cmd);

        exam.Id.Should().NotBeNullOrWhiteSpace();
        repo.Verify(r => r.AddExamToClassAsync("class-1", It.Is<Exam>(e => e == exam)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExamAlreadyHasId_DoesNotChangeId()
    {
        var repo = new Mock<IExamRepository>();
        repo.Setup(r => r.AddExamToClassAsync(It.IsAny<string>(), It.IsAny<Exam>()))
            .Returns(Task.CompletedTask);

        var handler = new AddExamCommandHandler(repo.Object);
        var exam = new Exam { Id = "fixed", Name = "P2" };
        var cmd = new AddExamCommand("class-2", exam);

        await handler.HandleAsync(cmd);

        exam.Id.Should().Be("fixed");
        repo.Verify(r => r.AddExamToClassAsync("class-2", It.Is<Exam>(e => e == exam)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExamIsNull_StillCallsRepositoryWithNull()
    {
        var repo = new Mock<IExamRepository>();
        repo.Setup(r => r.AddExamToClassAsync(It.IsAny<string>(), It.IsAny<Exam>()))
            .Returns(Task.CompletedTask);

        var handler = new AddExamCommandHandler(repo.Object);
        var cmd = new AddExamCommand("class-3", null!);

        await handler.HandleAsync(cmd);

        repo.Verify(r => r.AddExamToClassAsync("class-3", (Exam)null!), Times.Once);
    }
}

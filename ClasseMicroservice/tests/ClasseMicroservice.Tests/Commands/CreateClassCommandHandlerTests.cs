using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands;

public class CreateClassCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AssignsIds_WhenMissing_AndCallsRepository()
    {
        // Arrange
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<Class>())).Returns(Task.CompletedTask);

        var handler = new CreateClassCommandHandler(repo.Object);

        var cls = new Class
        {
            Id = null,
            ClassNumber = "101",
            Year = 2025,
            Semester = 2,
            Schedule = "Mon 10:00",
            Course = new Course { Id = null },
            Students = new List<Student> { new Student { Id = null }, new Student { Id = "pre" } },
            Professors = new List<Professor> { new Professor { Id = null } },
            Exams = new List<Exam> { new Exam { Id = null, Name = "P1" } }
        };

        var cmd = new CreateClassCommand(cls);

        // Act
        await handler.HandleAsync(cmd);

        // Assert
        cls.Id.Should().NotBeNullOrWhiteSpace();
        cls.Course.Id.Should().NotBeNullOrWhiteSpace();
        cls.Students[0].Id.Should().NotBeNullOrWhiteSpace();
        cls.Students[1].Id.Should().Be("pre");
        cls.Professors[0].Id.Should().NotBeNullOrWhiteSpace();
        cls.Exams[0].Id.Should().NotBeNullOrWhiteSpace();

        repo.Verify(r => r.CreateAsync(It.Is<Class>(c => c == cls)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NullClass_DoesNotThrow_StillCallsRepositoryWithNull()
    {
        // Arrange
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.CreateAsync(null!)).Returns(Task.CompletedTask);
        var handler = new CreateClassCommandHandler(repo.Object);
        var cmd = new CreateClassCommand(null!);

        // Act
        await handler.HandleAsync(cmd);

        // Assert
        repo.Verify(r => r.CreateAsync(null!), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NullCollections_And_NullCourse_AreHandled()
    {
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<Class>())).Returns(Task.CompletedTask);
        var handler = new CreateClassCommandHandler(repo.Object);

        var cls = new Class
        {
            Id = null,
            ClassNumber = "202",
            Year = 2026,
            Semester = 1,
            Schedule = "Tue 08:00",
            Course = null!,
            Students = null!,
            Professors = null!,
            Exams = null!
        };

        await handler.HandleAsync(new CreateClassCommand(cls));

        cls.Id.Should().NotBeNullOrWhiteSpace();
        // Course and collections remain null without throwing
        repo.Verify(r => r.CreateAsync(cls), Times.Once);
    }
}

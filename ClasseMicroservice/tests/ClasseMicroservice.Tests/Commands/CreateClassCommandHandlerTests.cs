using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Commands
{
    public class CreateClassCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_AssignsIds_WhenMissing()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            Class? captured = null;
            repo.Setup(r => r.CreateAsync(It.IsAny<Class>()))
                .Callback<Class>(c => captured = c)
                .Returns(Task.CompletedTask);

            var handler = new CreateClassCommandHandler(repo.Object);

            var cls = new Class
            {
                ClassNumber = "01",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon 10-12",
                Course = new Course { /* Id missing on purpose */ },
                Exams = new List<Exam> { new Exam { Name = "P1" } },
                Students = new List<Student> { new Student() },
                Professors = new List<Professor> { new Professor() }
            };

            await handler.HandleAsync(new CreateClassCommand(cls));

            repo.Verify(r => r.CreateAsync(It.IsAny<Class>()), Times.Once);
            captured.Should().NotBeNull();
            captured!.Id.Should().NotBeNullOrWhiteSpace();
            captured.Course.Id.Should().NotBeNullOrWhiteSpace();
            captured.Exams[0].Id.Should().NotBeNullOrWhiteSpace();
            captured.Students[0].Id.Should().NotBeNullOrWhiteSpace();
            captured.Professors[0].Id.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task HandleAsync_DoesNotOverrideExistingIds()
        {
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            Class? captured = null;
            repo.Setup(r => r.CreateAsync(It.IsAny<Class>()))
                .Callback<Class>(c => captured = c)
                .Returns(Task.CompletedTask);

            var handler = new CreateClassCommandHandler(repo.Object);

            var cls = new Class
            {
                Id = "CLASS-1",
                ClassNumber = "01",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon 10-12",
                Course = new Course { Id = "COURSE-1" },
                Exams = new List<Exam> { new Exam { Id = "EX-1", Name = "P1" } },
                Students = new List<Student> { new Student { Id = "ST-1" } },
                Professors = new List<Professor> { new Professor { Id = "PF-1" } }
            };

            await handler.HandleAsync(new CreateClassCommand(cls));

            repo.Verify(r => r.CreateAsync(It.IsAny<Class>()), Times.Once);
            captured!.Id.Should().Be("CLASS-1");
            captured.Course.Id.Should().Be("COURSE-1");
            captured.Exams[0].Id.Should().Be("EX-1");
            captured.Students[0].Id.Should().Be("ST-1");
            captured.Professors[0].Id.Should().Be("PF-1");
        }
    }
}


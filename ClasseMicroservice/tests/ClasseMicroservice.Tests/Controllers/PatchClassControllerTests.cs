using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

using ClasseMicroservice.API.Controllers;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Application.Commands;

namespace ClasseMicroservice.Tests.Controllers;

public class PatchClassControllerTests
{
    [Fact]
    public async Task PatchClass_ShouldApplyUpdates_AndCallUpdateHandler()
    {
        // Arrange
        var classId = "c1";
        var existing = new Class
        {
            Id = classId,
            ClassNumber = "101",
            Year = 2025,
            Semester = 1,
            Schedule = "Manha",
            Course = new Course { Id = "course1" },
            Exams = new List<Exam>(),
            Students = new List<Student>(),
            Professors = new List<Professor>()
        };

        var getClassesHandler = new Mock<IQueryHandler<GetClassesQuery, List<Class>>>();
        var getByIdHandler = new Mock<IQueryHandler<GetClassByIdQuery, Class>>();
        getByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>()))
            .ReturnsAsync(existing);

        var createHandler = new Mock<ICommandHandler<CreateClassCommand>>();

        UpdateClassCommand? capturedCommand = null;
        var updateHandler = new Mock<ICommandHandler<UpdateClassCommand>>();
        updateHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>()))
            .Callback<UpdateClassCommand>(cmd => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        var deleteHandler = new Mock<ICommandHandler<DeleteClassCommand>>();

        var controller = new ClassesController(
            getClassesHandler.Object,
            getByIdHandler.Object,
            createHandler.Object,
            updateHandler.Object,
            deleteHandler.Object
        );

        var updates = new Dictionary<string, object>
        {
            ["Year"] = JsonDocument.Parse("2026").RootElement,
            ["Schedule"] = JsonDocument.Parse("\"Noite\"").RootElement,
            ["Semester"] = JsonDocument.Parse("2").RootElement
        };

        // Act
        var result = await controller.PatchClass(classId, updates);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<Class>(ok.Value);
        returned.Year.Should().Be(2026);
        returned.Semester.Should().Be(2);
        returned.Schedule.Should().Be("Noite");

        capturedCommand.Should().NotBeNull();
        capturedCommand!.Id.Should().Be(classId);
        capturedCommand.Class.Year.Should().Be(2026);
        capturedCommand.Class.Semester.Should().Be(2);
        capturedCommand.Class.Schedule.Should().Be("Noite");

        updateHandler.Verify(h => h.HandleAsync(It.IsAny<UpdateClassCommand>()), Times.Once);
    }
}

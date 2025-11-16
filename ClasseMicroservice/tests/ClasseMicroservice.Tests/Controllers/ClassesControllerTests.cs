using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.API.Controllers;
using ClasseMicroservice.API.DTOs;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Controllers
{
    public class ClassesControllerTests
    {
        private (ClassesController ctrl, Mock<IQueryHandler<GetClassesQuery, List<Class>>> qAll,
            Mock<IQueryHandler<GetClassByIdQuery, Class>> qById,
            Mock<ICommandHandler<CreateClassCommand>> cCreate,
            Mock<ICommandHandler<UpdateClassCommand>> cUpdate,
            Mock<ICommandHandler<DeleteClassCommand>> cDelete) Make()
        {
            var qAll = new Mock<IQueryHandler<GetClassesQuery, List<Class>>>(MockBehavior.Strict);
            var qById = new Mock<IQueryHandler<GetClassByIdQuery, Class>>(MockBehavior.Strict);
            var cCreate = new Mock<ICommandHandler<CreateClassCommand>>(MockBehavior.Strict);
            var cUpdate = new Mock<ICommandHandler<UpdateClassCommand>>(MockBehavior.Strict);
            var cDelete = new Mock<ICommandHandler<DeleteClassCommand>>(MockBehavior.Strict);
            var ctrl = new ClassesController(qAll.Object, qById.Object, cCreate.Object, cUpdate.Object, cDelete.Object);
            return (ctrl, qAll, qById, cCreate, cUpdate, cDelete);
        }

        [Fact]
        public async Task CreateClass_ReturnsCreated_WithGeneratedId()
        {
            var (ctrl, _, _, cCreate, _, _) = Make();
            cCreate.Setup(h => h.HandleAsync(It.IsAny<CreateClassCommand>()))
                .Callback<CreateClassCommand>(cmd => cmd.Class.Id = "NEW-ID")
                .Returns(Task.CompletedTask);

            var dto = new CreateClassDto
            {
                ClassNumber = "01",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon",
                Course = new CreateCourseDto { Id = "COURSE-1" }
            };

            var result = await ctrl.CreateClass(dto) as CreatedAtRouteResult;
            result.Should().NotBeNull();
            result!.RouteName.Should().Be("GetClassById");
            ((Class)result.Value!).Id.Should().Be("NEW-ID");
        }

        [Fact]
        public async Task GetClassById_ReturnsNotFound_WhenNull()
        {
            var (ctrl, _, qById, _, _, _) = Make();
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);

            var result = await ctrl.GetClassById("X");
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PatchClass_UpdatesSimpleFields()
        {
            var (ctrl, _, qById, cCreate, cUpdate, _) = Make();
            var entity = new Class { Id = "ID-1", Year = 2024, Semester = 1, Schedule = "Old" };
            qById.Setup(h => h.HandleAsync(It.Is<GetClassByIdQuery>(q => q.Id == "ID-1"))).ReturnsAsync(entity);
            cUpdate.Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>())).Returns(Task.CompletedTask);

            var updates = new Dictionary<string, object>
            {
                ["Year"] = 2025,
                ["Schedule"] = "New"
            };

            var result = await ctrl.PatchClass("ID-1", updates) as OkObjectResult;
            result.Should().NotBeNull();
            var updated = (Class)result!.Value!;
            updated.Year.Should().Be(2025);
            updated.Schedule.Should().Be("New");
        }

        [Fact]
        public async Task AddExam_AddsExamAndReturnsCreated()
        {
            var (ctrl, _, qById, _, cUpdate, _) = Make();
            var entity = new Class { Id = "ID-1", Exams = new List<Exam>() };
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync(entity);
            cUpdate.Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>())).Returns(Task.CompletedTask);

            var result = await ctrl.AddExam("ID-1", new Exam { Name = "P1" }) as CreatedAtActionResult;
            result.Should().NotBeNull();
            entity.Exams.Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteExam_RemovesAndReturnsNoContent()
        {
            var (ctrl, _, qById, _, cUpdate, _) = Make();
            var entity = new Class { Id = "ID-1", Exams = new List<Exam> { new Exam { Id = "EX-1" } } };
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync(entity);
            cUpdate.Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>())).Returns(Task.CompletedTask);

            var result = await ctrl.DeleteExam("ID-1", "EX-1");
            result.Should().BeOfType<NoContentResult>();
            entity.Exams.Should().BeEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.API.Controllers;
using ClasseMicroservice.Application.Commands;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Controllers
{
    public class MoreClassesControllerTests
    {
        private (ClassesController ctrl, Mock<IQueryHandler<GetClassesQuery, List<Class>>> qAll,
            Mock<IQueryHandler<GetClassByIdQuery, Class>> qById,
            Mock<ICommandHandler<CreateClassCommand>> cCreate,
            Mock<ICommandHandler<UpdateClassCommand>> cUpdate,
            Mock<ICommandHandler<DeleteClassCommand>> cDelete) Make()
        {
            var qAll = new Mock<IQueryHandler<GetClassesQuery, List<Class>>>();
            var qById = new Mock<IQueryHandler<GetClassByIdQuery, Class>>();
            var cCreate = new Mock<ICommandHandler<CreateClassCommand>>();
            var cUpdate = new Mock<ICommandHandler<UpdateClassCommand>>();
            var cDelete = new Mock<ICommandHandler<DeleteClassCommand>>();
            var ctrl = new ClassesController(qAll.Object, qById.Object, cCreate.Object, cUpdate.Object, cDelete.Object);
            return (ctrl, qAll, qById, cCreate, cUpdate, cDelete);
        }

        [Fact]
        public async Task GetClasses_Should_ReturnOk()
        {
            var (ctrl, qAll, _, _, _, _) = Make();
            qAll.Setup(h => h.HandleAsync(It.IsAny<GetClassesQuery>())).ReturnsAsync(new List<Class> { new Class { Id = "1" } });
            var result = await ctrl.GetClasses(2025, 2, "COURSE", 1, 5) as OkObjectResult;
            result.Should().NotBeNull();
            ((List<Class>)result!.Value!).Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateClass_Should_SetId_And_ReturnOk()
        {
            var (ctrl, _, _, _, cUpdate, _) = Make();
            cUpdate.Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>())).Returns(Task.CompletedTask);
            var body = new Class { Id = null, ClassNumber = "A", Year = 2025, Semester = 2, Schedule = "S" };
            var result = await ctrl.UpdateClass("RID", body) as OkObjectResult;
            result.Should().NotBeNull();
            ((Class)result!.Value!).Id.Should().Be("RID");
        }

        [Fact]
        public async Task UpdateClass_Should_ReturnBadRequest_When_Null()
        {
            var (ctrl, _, _, _, _, _) = Make();
            var result = await ctrl.UpdateClass("RID", null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteClass_Should_ReturnNoContent()
        {
            var (ctrl, _, _, _, _, cDelete) = Make();
            cDelete.Setup(h => h.HandleAsync(It.IsAny<DeleteClassCommand>())).Returns(Task.CompletedTask);
            var result = await ctrl.DeleteClass("RID");
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task AddExam_Should_ReturnBadRequest_When_Null()
        {
            var (ctrl, _, _, _, _, _) = Make();
            var result = await ctrl.AddExam("RID", null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddExam_Should_ReturnNotFound_When_ClassMissing()
        {
            var (ctrl, _, qById, _, _, _) = Make();
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);
            var result = await ctrl.AddExam("RID", new Exam { Name = "X" });
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetExamById_Should_ReturnNotFound_When_ClassOrExamMissing()
        {
            var (ctrl, _, qById, _, _, _) = Make();
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);
            (await ctrl.GetExamById("RID", "EID")).Should().BeOfType<NotFoundResult>();

            var cls = new Class { Id = "RID", Exams = new List<Exam>() };
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync(cls);
            (await ctrl.GetExamById("RID", "EID")).Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateExam_BadRequest_And_NotFound_Flows()
        {
            var (ctrl, _, qById, _, cUpdate, _) = Make();
            (await ctrl.UpdateExam("RID", "EID", null!)).Should().BeOfType<BadRequestObjectResult>();

            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);
            (await ctrl.UpdateExam("RID", "EID", new Exam { Name = "X" })).Should().BeOfType<NotFoundResult>();

            var cls = new Class { Id = "RID", Exams = new List<Exam>() };
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync(cls);
            (await ctrl.UpdateExam("RID", "EID", new Exam { Name = "X" })).Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PatchExam_400_404_And_200()
        {
            var (ctrl, _, qById, _, cUpdate, _) = Make();
            (await ctrl.PatchExam("RID", "EID", null!)).Should().BeOfType<BadRequestObjectResult>();

            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);
            (await ctrl.PatchExam("RID", "EID", new Dictionary<string, object> { ["Name"] = "X" })).Should().BeOfType<NotFoundResult>();

            var cls = new Class { Id = "RID", Exams = new List<Exam>() };
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync(cls);
            (await ctrl.PatchExam("RID", "EID", new Dictionary<string, object> { ["Name"] = "X" })).Should().BeOfType<NotFoundResult>();

            var exam = new Exam { Id = "EID", Name = "Old", Weight = 10 };
            cls.Exams.Add(exam);
            cUpdate.Setup(h => h.HandleAsync(It.IsAny<UpdateClassCommand>())).Returns(Task.CompletedTask);
            var ok = await ctrl.PatchExam("RID", "EID", new Dictionary<string, object> { ["Name"] = "New", ["Weight"] = 20 });
            var okObj = Assert.IsType<OkObjectResult>(ok);
            var updated = Assert.IsType<Exam>(okObj.Value);
            updated.Name.Should().Be("New");
            updated.Weight.Should().Be(20);
        }

        [Fact]
        public async Task PatchClass_Should_ReturnBadRequest_When_Empty()
        {
            var (ctrl, _, _, _, _, _) = Make();
            var res = await ctrl.PatchClass("RID", new Dictionary<string, object>());
            res.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task PatchClass_Should_ReturnNotFound_When_ClassMissing()
        {
            var (ctrl, _, qById, _, _, _) = Make();
            qById.Setup(h => h.HandleAsync(It.IsAny<GetClassByIdQuery>())).ReturnsAsync((Class?)null);
            var res = await ctrl.PatchClass("RID", new Dictionary<string, object> { ["Year"] = 2025 });
            res.Should().BeOfType<NotFoundResult>();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Queries
{
    public class GetClassesQueryHandlerTests
    {
        private static List<Class> Seed()
        {
            return new List<Class>
            {
                new Class { Id = "1", Year = 2024, Semester = 1, ClassNumber = "A", Schedule = "Mon", Course = new Course { Id = "C1" } },
                new Class { Id = "2", Year = 2024, Semester = 2, ClassNumber = "B", Schedule = "Tue", Course = new Course { Id = "C1" } },
                new Class { Id = "3", Year = 2025, Semester = 1, ClassNumber = "C", Schedule = "Wed", Course = new Course { Id = "C2" } },
                new Class { Id = "4", Year = 2025, Semester = 2, ClassNumber = "D", Schedule = "Thu", Course = new Course { Id = "C3" } },
                new Class { Id = "5", Year = 2025, Semester = 2, ClassNumber = "E", Schedule = "Fri", Course = new Course { Id = "C1" } }
            };
        }

        [Fact]
        public async Task FiltersByYearSemesterAndCourse_WithPagination()
        {
            var data = Seed();
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);

            var handler = new GetClassesQueryHandler(repo.Object);

            var query = new GetClassesQuery(year: 2025, semester: 2, courseId: "C1", page: 1, size: 1);
            var result = await handler.HandleAsync(query);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be("5");

            // page 2 should be empty (only one item matches)
            var resultPage2 = await handler.HandleAsync(new GetClassesQuery(2025, 2, "C1", page: 2, size: 1));
            resultPage2.Should().BeEmpty();
        }

        [Fact]
        public async Task ReturnsPagedResults_WhenNoFilters()
        {
            var data = Seed();
            var repo = new Mock<IClassRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);

            var handler = new GetClassesQueryHandler(repo.Object);
            var result = await handler.HandleAsync(new GetClassesQuery(page: 2, size: 2));

            result.Select(c => c.Id).Should().BeEquivalentTo(new[] { "3", "4" });
        }
    }
}

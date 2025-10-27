using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Queries;

public class GetClassQueriesHandlersTests
{
    [Fact]
    public async Task GetById_Handler_ReturnsEntityFromRepository()
    {
        var expected = new Class { Id = "abc" };
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetByIdAsync("abc")).ReturnsAsync(expected);

        var handler = new GetClassByIdQueryHandler(repo.Object);
        var result = await handler.HandleAsync(new GetClassByIdQuery("abc"));

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetClasses_Handler_AppliesFiltersAndPagination()
    {
        // data
        var courseA = new Course { Id = "course-a" };
        var courseB = new Course { Id = "course-b" };
        var data = new List<Class>
        {
            new() { Id = "1", Year = 2024, Semester = 1, Course = courseA },
            new() { Id = "2", Year = 2024, Semester = 2, Course = courseA },
            new() { Id = "3", Year = 2025, Semester = 1, Course = courseA },
            new() { Id = "4", Year = 2025, Semester = 1, Course = courseB },
            new() { Id = "5", Year = 2025, Semester = 1, Course = courseA },
        };

        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);

        var handler = new GetClassesQueryHandler(repo.Object);

        // filter year 2025, semester 1, course-a; then page size 1, page 2 -> expect the second item of the filtered list
        var query = new GetClassesQuery(year: 2025, semester: 1, courseId: "course-a", page: 2, size: 1);
        var result = await handler.HandleAsync(query);

        // filtered list should be ids 3 and 5; page 2 size 1 => only id 5
        result.Should().HaveCount(1);
        result.First().Id.Should().Be("5");
    }

    [Fact]
    public async Task GetClasses_Handler_YearOnly_Filter()
    {
        var data = new List<Class>
        {
            new() { Id = "1", Year = 2024, Semester = 1, Course = new Course { Id = "A" } },
            new() { Id = "2", Year = 2025, Semester = 2, Course = new Course { Id = "B" } },
        };
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);
        var handler = new GetClassesQueryHandler(repo.Object);

        var result = await handler.HandleAsync(new GetClassesQuery(year: 2025));
        result.Should().HaveCount(1).And.OnlyContain(c => c.Id == "2");
    }

    [Fact]
    public async Task GetClasses_Handler_SemesterOnly_Filter()
    {
        var data = new List<Class>
        {
            new() { Id = "1", Year = 2024, Semester = 1, Course = new Course { Id = "A" } },
            new() { Id = "2", Year = 2025, Semester = 2, Course = new Course { Id = "B" } },
            new() { Id = "3", Year = 2025, Semester = 1, Course = new Course { Id = "B" } },
        };
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);
        var handler = new GetClassesQueryHandler(repo.Object);

        var result = await handler.HandleAsync(new GetClassesQuery(semester: 1));
        result.Select(c => c.Id).Should().BeEquivalentTo(new[] { "1", "3" });
    }

    [Fact]
    public async Task GetClasses_Handler_CourseOnly_Filter()
    {
        var data = new List<Class>
        {
            new() { Id = "1", Year = 2024, Semester = 1, Course = new Course { Id = "A" } },
            new() { Id = "2", Year = 2025, Semester = 2, Course = new Course { Id = "B" } },
            new() { Id = "3", Year = 2025, Semester = 1, Course = new Course { Id = "B" } },
        };
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);
        var handler = new GetClassesQueryHandler(repo.Object);

        var result = await handler.HandleAsync(new GetClassesQuery(courseId: "B"));
        result.Select(c => c.Id).Should().BeEquivalentTo(new[] { "2", "3" });
    }

    [Fact]
    public async Task GetClasses_Handler_Pagination_BeyondRange_ReturnsEmpty()
    {
        var data = new List<Class>
        {
            new() { Id = "1", Year = 2024, Semester = 1, Course = new Course { Id = "A" } },
            new() { Id = "2", Year = 2024, Semester = 1, Course = new Course { Id = "A" } },
        };
        var repo = new Mock<IClassRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(data);
        var handler = new GetClassesQueryHandler(repo.Object);

        // Two items total; page 3 of size 1 => empty
        var result = await handler.HandleAsync(new GetClassesQuery(page: 3, size: 1));
        result.Should().BeEmpty();
    }
}

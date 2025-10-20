using ClasseMicroservice.Application.Queries;
using FluentAssertions;
using Xunit;

namespace ClasseMicroservice.Tests.Queries;

public class GetClassesQueryTests
{
    [Fact]
    public void Constructor_Normalizes_Page_Size_And_CourseId()
    {
        var q = new GetClassesQuery(year: null, semester: null, courseId: "   ", page: 0, size: -5);

        q.Page.Should().Be(1);
        q.Size.Should().Be(10);
        q.CourseId.Should().BeNull();
    }
}

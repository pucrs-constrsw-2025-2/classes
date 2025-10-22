using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Application.Queries;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClasseMicroservice.Tests.Queries;

public class GetExamsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsExamsFromRepository()
    {
        var expected = new List<Exam> { new Exam { Id = "e1" }, new Exam { Id = "e2" } };
        var repo = new Mock<IExamRepository>();
        repo.Setup(r => r.GetExamsByClassIdAsync("cls"))
            .ReturnsAsync(expected);

        var handler = new GetExamsQueryHandler(repo.Object);
        var result = await handler.HandleAsync(new GetExamsQuery("cls"));

        result.Should().BeSameAs(expected);
    }
}

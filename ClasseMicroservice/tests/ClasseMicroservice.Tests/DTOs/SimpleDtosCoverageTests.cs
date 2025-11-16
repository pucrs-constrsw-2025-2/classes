using System.Text.Json;
using ClasseMicroservice.API.DTOs;
using FluentAssertions;
using Xunit;

namespace ClasseMicroservice.Tests.DTOs
{
    public class SimpleDtosCoverageTests
    {
        [Fact]
        public void CreateProfessorDto_Serialization_Works()
        {
            var dto = new CreateProfessorDto { Id = "P1" };
            var json = JsonSerializer.Serialize(dto);
            json.Should().Contain("P1");
            var back = JsonSerializer.Deserialize<CreateProfessorDto>(json);
            back!.Id.Should().Be("P1");
        }

        [Fact]
        public void CreateStudentDto_Serialization_Works()
        {
            var dto = new CreateStudentDto { Id = "S1" };
            var json = JsonSerializer.Serialize(dto);
            json.Should().Contain("S1");
            var back = JsonSerializer.Deserialize<CreateStudentDto>(json);
            back!.Id.Should().Be("S1");
        }
    }
}

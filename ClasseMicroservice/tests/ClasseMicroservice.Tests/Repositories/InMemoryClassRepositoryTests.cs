using System.Threading.Tasks;
using ClasseMicroservice.API.Repositories;
using ClasseMicroservice.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ClasseMicroservice.Tests.Repositories
{
    public class InMemoryClassRepositoryTests
    {
        [Fact]
        public async Task Create_Assigns_Id_When_Empty()
        {
            var repo = new InMemoryClassRepository();
            var c = new Class { ClassNumber = "N", Year = 2025, Semester = 1, Schedule = "S" };
            await repo.CreateAsync(c);
            c.Id.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Delete_NonExisting_Does_Not_Throw()
        {
            var repo = new InMemoryClassRepository();
            await repo.DeleteAsync("does-not-exist");
            (await repo.GetAllAsync()).Should().BeEmpty();
        }

        [Fact]
        public async Task Update_NonExisting_Does_Not_Throw()
        {
            var repo = new InMemoryClassRepository();
            await repo.UpdateAsync("missing", new Class { Id = "missing" });
            (await repo.GetByIdAsync("missing")).Should().BeNull();
        }
    }
}

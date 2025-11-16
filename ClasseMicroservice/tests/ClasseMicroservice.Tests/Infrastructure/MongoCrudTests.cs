using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace ClasseMicroservice.Tests.Infrastructure
{
    public class MongoCrudTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IConfiguration _cfg;

        public MongoCrudTests()
        {
            _runner = MongoDbRunner.Start();
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDb"] = _runner.ConnectionString,
                ["MongoDbSettings:DatabaseName"] = "classes-it",
                ["MongoDbSettings:CollectionName"] = "Classes"
            };
            _cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        [Fact]
        public async Task MigrationService_Should_Create_Collections_And_Indexes()
        {
            var svc = new ClasseMicroservice.API.Data.MigrationService(_cfg);
            await svc.EnsureCollectionsAndIndexesAsync();

            var client = new MongoClient(_cfg.GetConnectionString("MongoDb"));
            var db = client.GetDatabase(_cfg.GetValue<string>("MongoDbSettings:DatabaseName"));
            var names = await db.ListCollectionNames().ToListAsync();
            names.Should().Contain(new[] { "Classes", "Exams", "Students", "Professors", "Courses" });
        }

        [Fact]
        public async Task ClassRepository_CRUD_Works()
        {
            var repo = new ClassRepository(_cfg);
            var cls = new Class
            {
                Id = "C-IT-1",
                ClassNumber = "IT-1",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon",
                Exams = new(), Students = new(), Professors = new()
            };

            await repo.CreateAsync(cls);
            (await repo.GetAllAsync()).Should().ContainSingle(c => c.Id == "C-IT-1");
            (await repo.GetByIdAsync("C-IT-1")).Should().NotBeNull();

            cls.Schedule = "Tue";
            await repo.UpdateAsync(cls.Id!, cls);
            (await repo.GetByIdAsync("C-IT-1")).Schedule.Should().Be("Tue");

            await repo.DeleteAsync("C-IT-1");
            (await repo.GetByIdAsync("C-IT-1")).Should().BeNull();
        }

        [Fact]
        public async Task ExamRepository_Flow_Works()
        {
            var classRepo = new ClassRepository(_cfg);
            var examRepo = new ExamRepository(_cfg);

            var cls = new Class
            {
                Id = "C-IT-2",
                ClassNumber = "IT-2",
                Year = 2025,
                Semester = 2,
                Schedule = "Wed",
                Exams = new(), Students = new(), Professors = new()
            };
            await classRepo.CreateAsync(cls);

            var ex = new Exam { Id = "E-1", Name = "Exam 1", Date = DateTime.UtcNow, Weight = 30 };
            await examRepo.AddExamToClassAsync(cls.Id!, ex);
            (await examRepo.GetExamsByClassIdAsync(cls.Id!)).Should().ContainSingle(e => e.Id == "E-1");
            (await examRepo.GetExamByIdAsync("E-1")).Should().NotBeNull();

            ex.Name = "Exam 1 Updated";
            await examRepo.UpdateExamAsync(ex.Id!, ex);
            (await examRepo.GetExamByIdAsync("E-1")).Name.Should().Be("Exam 1 Updated");

            await examRepo.DeleteExamAsync("E-1");
            (await examRepo.GetExamByIdAsync("E-1")).Should().BeNull();
        }

        public void Dispose()
        {
            _runner?.Dispose();
        }
    }
}

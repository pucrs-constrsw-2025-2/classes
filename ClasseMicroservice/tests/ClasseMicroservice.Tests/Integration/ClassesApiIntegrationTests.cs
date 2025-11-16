using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClasseMicroservice.API.DTOs;
using ClasseMicroservice.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClasseMicroservice.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ClassesApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ClassesApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer abc.def.ghi");
        }

        [Fact]
        public async Task Create_Then_GetById_And_List()
        {
            var dto = new CreateClassDto
            {
                ClassNumber = "INT-01",
                Year = 2025,
                Semester = 2,
                Schedule = "Mon 10:00",
                Course = new CreateCourseDto { Id = "COURSE-INT" }
            };

            var respCreate = await _client.PostAsJsonAsync("/api/v1/classes", dto);
            respCreate.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await respCreate.Content.ReadFromJsonAsync<Class>();
            created.Should().NotBeNull();
            created!.Id.Should().NotBeNullOrWhiteSpace();

            var respGet = await _client.GetAsync($"/api/v1/classes/{created.Id}");
            respGet.StatusCode.Should().Be(HttpStatusCode.OK);
            var fetched = await respGet.Content.ReadFromJsonAsync<Class>();
            fetched!.Id.Should().Be(created.Id);

            var respList = await _client.GetAsync("/api/v1/classes?page=1&size=10");
            respList.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await respList.Content.ReadFromJsonAsync<List<Class>>();
            list.Should().NotBeNull();
            list!.Should().ContainSingle(c => c.Id == created.Id);
        }

        [Fact]
        public async Task Create_With_ExamDto_Should_Include_Exam()
        {
            var dto = new CreateClassDto
            {
                ClassNumber = "INT-EXAM",
                Year = 2025,
                Semester = 2,
                Schedule = "Thu 14:00",
                Exam = new CreateExamDto { Name = "Exame DTO", Weight = 40, Date = DateTime.UtcNow }
            };

            var resp = await _client.PostAsJsonAsync("/api/v1/classes", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await resp.Content.ReadFromJsonAsync<Class>();
            created!.Exams.Should().NotBeNull();
            created!.Exams!.Should().ContainSingle(e => e.Name == "Exame DTO" && e.Weight == 40);

            var listExams = await _client.GetFromJsonAsync<List<Exam>>($"/api/v1/classes/{created.Id}/exams");
            listExams!.Should().ContainSingle(e => e.Name == "Exame DTO" && e.Weight == 40);
        }

        [Fact]
        public async Task Patch_And_Exam_Subresource_Flow()
        {
            // Create a class first
            var respCreate = await _client.PostAsJsonAsync("/api/v1/classes", new CreateClassDto
            {
                ClassNumber = "INT-02",
                Year = 2024,
                Semester = 1,
                Schedule = "Tue 08:00"
            });
            respCreate.EnsureSuccessStatusCode();
            var created = await respCreate.Content.ReadFromJsonAsync<Class>();

            // Patch class
            var patch = new Dictionary<string, object> { ["Year"] = 2026, ["Schedule"] = "Fri 09:00" };
            var respPatch = await _client.PatchAsJsonAsync($"/api/v1/classes/{created!.Id}", patch);
            respPatch.StatusCode.Should().Be(HttpStatusCode.OK);
            var patched = await respPatch.Content.ReadFromJsonAsync<Class>();
            patched!.Year.Should().Be(2026);
            patched.Schedule.Should().Be("Fri 09:00");

            // Add exam
            var exam = new Exam { Name = "Prova 1", Date = DateTime.UtcNow, Weight = 50 };
            var respAddExam = await _client.PostAsJsonAsync($"/api/v1/classes/{created.Id}/exams", exam);
            respAddExam.StatusCode.Should().Be(HttpStatusCode.Created);

            // List exams
            var respList = await _client.GetAsync($"/api/v1/classes/{created.Id}/exams");
            respList.StatusCode.Should().Be(HttpStatusCode.OK);
            var exams = await respList.Content.ReadFromJsonAsync<List<Exam>>();
            exams.Should().NotBeNull();
            exams!.Should().ContainSingle(e => e.Name == "Prova 1" && e.Weight == 50);

            // Get exam by id
            var examId = exams![0].Id;
            var respGetExam = await _client.GetAsync($"/api/v1/classes/{created.Id}/exams/{examId}");
            respGetExam.StatusCode.Should().Be(HttpStatusCode.OK);
            var single = await respGetExam.Content.ReadFromJsonAsync<Exam>();
            single!.Name.Should().Be("Prova 1");

            // Update exam
            var updated = new Exam { Name = "Prova 1 Editada", Date = single.Date, Weight = 60 };
            var respPut = await _client.PutAsJsonAsync($"/api/v1/classes/{created.Id}/exams/{examId}", updated);
            respPut.StatusCode.Should().Be(HttpStatusCode.OK);
            var afterPut = await respPut.Content.ReadFromJsonAsync<Exam>();
            afterPut!.Weight.Should().Be(60);

            // Delete exam
            var respDel = await _client.DeleteAsync($"/api/v1/classes/{created.Id}/exams/{examId}");
            respDel.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var respGetAfterDel = await _client.GetAsync($"/api/v1/classes/{created.Id}/exams/{examId}");
            respGetAfterDel.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return401_Without_Authorization_Header()
        {
            var anonClient = _factory.CreateClient();
            var resp = await anonClient.GetAsync("/api/v1/classes");
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_Should_Return404_When_NotFound()
        {
            var resp = await _client.GetAsync("/api/v1/classes/does-not-exist");
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Should_Return400_When_NullBody()
        {
            var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("/api/v1/classes", content);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_Should_Return400_When_EmptyUpdates()
        {
            // create a class first
            var created = await (await _client.PostAsJsonAsync("/api/v1/classes", new CreateClassDto
            {
                ClassNumber = "INT-03",
                Year = 2025,
                Semester = 1,
                Schedule = "Wed 11:00"
            })).Content.ReadFromJsonAsync<Class>();

            var empty = new Dictionary<string, object>();
            var resp = await _client.PatchAsJsonAsync($"/api/v1/classes/{created!.Id}", empty);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Swagger_Should_Be_Available_When_Enabled()
        {
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var dict = new Dictionary<string, string?>
                    {
                        ["EnableSwagger"] = "true",
                        ["ENABLE_SWAGGER"] = "true"
                    };
                    cfg.AddInMemoryCollection(dict!);
                });
            });

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer abc.def.ghi");
            var resp = await client.GetAsync("/swagger/index.html");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Health_Endpoint_Should_Return_Up()
        {
            var resp = await _client.GetAsync("/health");
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
            var json = await resp.Content.ReadAsStringAsync();
            json.Should().Contain("\"status\":");
        }

        [Fact]
        public async Task Metrics_Endpoint_Should_Be_Accessible()
        {
            var resp = await _client.GetAsync("/metrics");
            // Depending on exporter startup, metrics endpoint should exist
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

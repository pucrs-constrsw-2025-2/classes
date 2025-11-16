using System;
using System.Text.Json;
using Xunit;

namespace ClasseMicroservice.Tests.Models
{
    public class ApiModelsCoverageTests
    {
        [Fact]
        public void Student_Model_SetGet_Serializes()
        {
            var s = new ClasseMicroservice.API.Models.Student { Id = "stu-123" };
            Assert.Equal("stu-123", s.Id);

            var json = JsonSerializer.Serialize(s);
            Assert.Contains("stu-123", json);
            var back = JsonSerializer.Deserialize<ClasseMicroservice.API.Models.Student>(json);
            Assert.Equal("stu-123", back!.Id);
        }

        [Fact]
        public void Professor_Model_SetGet_Serializes()
        {
            var p = new ClasseMicroservice.API.Models.Professor { Id = "prof-456" };
            Assert.Equal("prof-456", p.Id);

            var json = JsonSerializer.Serialize(p);
            Assert.Contains("prof-456", json);
            var back = JsonSerializer.Deserialize<ClasseMicroservice.API.Models.Professor>(json);
            Assert.Equal("prof-456", back!.Id);
        }

        [Fact]
        public void Course_Model_SetGet_Serializes()
        {
            var c = new ClasseMicroservice.API.Models.Course { Id = "course-789" };
            Assert.Equal("course-789", c.Id);

            var json = JsonSerializer.Serialize(c);
            Assert.Contains("course-789", json);
            var back = JsonSerializer.Deserialize<ClasseMicroservice.API.Models.Course>(json);
            Assert.Equal("course-789", back!.Id);
        }

        [Fact]
        public void Exam_Model_SetGet_Serializes()
        {
            var when = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var e = new ClasseMicroservice.API.Models.Exam
            {
                Id = "exam-001",
                Name = "Midterm",
                Date = when,
                Weight = 40
            };

            Assert.Equal("exam-001", e.Id);
            Assert.Equal("Midterm", e.Name);
            Assert.Equal(when, e.Date);
            Assert.Equal(40, e.Weight);

            var json = JsonSerializer.Serialize(e);
            Assert.Contains("exam-001", json);
            Assert.Contains("Midterm", json);
            var back = JsonSerializer.Deserialize<ClasseMicroservice.API.Models.Exam>(json);
            Assert.Equal("exam-001", back!.Id);
            Assert.Equal("Midterm", back.Name);
            Assert.Equal(40, back.Weight);
            // DateTime round-trip format may change; compare by ticks
            Assert.Equal(when.ToUniversalTime().Ticks, back.Date.ToUniversalTime().Ticks);
        }

        [Fact]
        public void DatabaseSettings_SetGet_Serializes()
        {
            var ds = new ClasseMicroservice.API.DatabaseSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "school"
            };

            Assert.Equal("mongodb://localhost:27017", ds.ConnectionString);
            Assert.Equal("school", ds.DatabaseName);

            var json = JsonSerializer.Serialize(ds);
            Assert.Contains("mongodb://localhost:27017", json);
            Assert.Contains("school", json);
            var back = JsonSerializer.Deserialize<ClasseMicroservice.API.DatabaseSettings>(json);
            Assert.Equal("mongodb://localhost:27017", back!.ConnectionString);
            Assert.Equal("school", back.DatabaseName);
        }
    }
}

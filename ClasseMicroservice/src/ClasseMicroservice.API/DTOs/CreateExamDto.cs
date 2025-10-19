using System;

namespace ClasseMicroservice.API.DTOs
{
    public class CreateExamDto
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Weight { get; set; }
    }
}

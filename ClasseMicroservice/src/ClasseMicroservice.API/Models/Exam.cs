using System;

namespace ClasseMicroservice.API.Models
{
    public class Exam
    {
        public string Id { get; set; } // UUID
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Weight { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace ClasseMicroservice.API.DTOs
{
    public class CreateClassDto
    {
        public string? ClassNumber { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
        public string? Schedule { get; set; }

        // Course opcional
        public CreateCourseDto? Course { get; set; }

        // Permitimos 0 ou 1 exam/student/professor nesta DTO simplificada
        public CreateExamDto? Exam { get; set; }
        public CreateStudentDto? Student { get; set; }
        public CreateProfessorDto? Professor { get; set; }
    }
}

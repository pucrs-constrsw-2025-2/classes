using System.Collections.Generic;

namespace ClasseMicroservice.Application.Queries
{
    public class GetClassesQuery
    {
        public int? Year { get; set; }
        public int? Semester { get; set; }
        public string CourseId { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }

        public GetClassesQuery(int? year = null, int? semester = null, string courseId = null, int page = 1, int size = 10)
        {
            Year = year;
            Semester = semester;
            CourseId = courseId;
            Page = page;
            Size = size;
        }
    }
}
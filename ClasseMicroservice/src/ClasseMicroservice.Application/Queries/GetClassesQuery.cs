namespace ClasseMicroservice.Application.Queries
{
    public sealed class GetClassesQuery
    {
        public int? Year { get; }
        public int? Semester { get; }
        public string CourseId { get; }
        public int Page { get; }
        public int Size { get; }

        public GetClassesQuery(int? year = null, int? semester = null, string courseId = null, int page = 1, int size = 10)
        {
            Year = year;
            Semester = semester;
            CourseId = string.IsNullOrWhiteSpace(courseId) ? null : courseId; // pode ser null
            Page = page <= 0 ? 1 : page;
            Size = size <= 0 ? 10 : size;
        }
    }
}
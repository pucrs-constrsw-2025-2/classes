namespace ClasseMicroservice.Application.Queries
{
    public class GetExamsQuery
    {
        public string ClassId { get; set; }

        public GetExamsQuery(string classId)
        {
            ClassId = classId;
        }
    }
}
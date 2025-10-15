namespace ClasseMicroservice.Application.Queries
{
    public class GetClassByIdQuery
    {
        public string Id { get; set; }
        public GetClassByIdQuery(string id)
        {
            Id = id;
        }
    }
}

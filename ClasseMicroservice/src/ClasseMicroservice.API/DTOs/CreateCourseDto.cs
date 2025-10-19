namespace ClasseMicroservice.API.DTOs
{
    public class CreateCourseDto
    {
        // Você pode passar o Id do course existente ou null para que o backend gere
        public string? Id { get; set; }
    }
}

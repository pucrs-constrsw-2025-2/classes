namespace ClasseMicroservice.Application.Commands
{
    public class DeleteClassCommand
    {
        public string Id { get; set; }

        public DeleteClassCommand(string id)
        {
            Id = id;
        }
    }
}
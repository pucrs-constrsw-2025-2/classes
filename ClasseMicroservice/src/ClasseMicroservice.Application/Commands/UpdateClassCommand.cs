using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.Application.Commands
{
    public class UpdateClassCommand
    {
        public string Id { get; set; }
        public Class Class { get; set; }

        public UpdateClassCommand(string id, Class @class)
        {
            Id = id;
            Class = @class;
        }
    }
}
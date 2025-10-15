using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.Application.Commands
{
    public class CreateClassCommand
    {
        public Class Class { get; set; }
        public CreateClassCommand(Class @class)
        {
            Class = @class;
        }
    }
}

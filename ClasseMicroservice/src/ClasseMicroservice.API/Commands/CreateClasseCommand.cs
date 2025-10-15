using ClasseMicroservice.API.Models;

namespace ClasseMicroservice.API.Commands
{
    public class CreateClassCommand
    {
        public Classe Class { get; set; }
        public CreateClassCommand(Classe @class)
        {
            Class = @class;
        }
    }
}

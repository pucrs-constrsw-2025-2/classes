using System.Threading.Tasks;
using ClasseMicroservice.API.Models;
using ClasseMicroservice.API.Data;

namespace ClasseMicroservice.API.Commands
{
    public class CreateClassCommandHandler : ICommandHandler<CreateClassCommand>
    {
        private readonly ClasseRepository _classeRepository;
        public CreateClassCommandHandler(ClasseRepository classeRepository)
        {
            _classeRepository = classeRepository;
        }
        public async Task HandleAsync(CreateClassCommand command)
        {
            await _classeRepository.Create(command.Class);
        }
    }
}

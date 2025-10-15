using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Commands
{
    public class CreateClassCommandHandler : ICommandHandler<CreateClassCommand>
    {
        private readonly IClassRepository _classRepository;
        public CreateClassCommandHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }
        public async Task HandleAsync(CreateClassCommand command)
        {
            await _classRepository.CreateAsync(command.Class);
        }
    }
}

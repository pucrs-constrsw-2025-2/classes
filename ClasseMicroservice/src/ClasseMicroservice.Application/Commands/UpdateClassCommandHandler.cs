using System.Threading.Tasks;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Commands
{
    public class UpdateClassCommandHandler : ICommandHandler<UpdateClassCommand>
    {
        private readonly IClassRepository _classRepository;

        public UpdateClassCommandHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task HandleAsync(UpdateClassCommand command)
        {
            await _classRepository.UpdateAsync(command.Id, command.Class);
        }
    }
}
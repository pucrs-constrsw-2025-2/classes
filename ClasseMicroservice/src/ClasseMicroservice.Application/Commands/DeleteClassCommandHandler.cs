using System.Threading.Tasks;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Commands
{
    public class DeleteClassCommandHandler : ICommandHandler<DeleteClassCommand>
    {
        private readonly IClassRepository _classRepository;

        public DeleteClassCommandHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task HandleAsync(DeleteClassCommand command)
        {
            await _classRepository.DeleteAsync(command.Id);
        }
    }
}
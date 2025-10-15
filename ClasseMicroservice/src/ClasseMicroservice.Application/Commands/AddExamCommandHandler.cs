using System.Threading.Tasks;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Commands
{
    public class AddExamCommandHandler : ICommandHandler<AddExamCommand>
    {
        private readonly IExamRepository _examRepository;

        public AddExamCommandHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task HandleAsync(AddExamCommand command)
        {
            await _examRepository.AddExamToClassAsync(command.ClassId, command.Exam);
        }
    }
}
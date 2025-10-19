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
            var exam = command.Exam;
            if (exam != null && string.IsNullOrWhiteSpace(exam.Id))
                exam.Id = System.Guid.NewGuid().ToString();

            await _examRepository.AddExamToClassAsync(command.ClassId, exam);
        }
    }
}
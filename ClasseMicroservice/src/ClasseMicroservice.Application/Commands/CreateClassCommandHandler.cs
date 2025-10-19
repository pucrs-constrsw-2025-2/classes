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
            // Ensure the Class and its nested entities have IDs assigned by backend (UUID)
            var cls = command.Class;
            if (cls != null)
            {
                if (string.IsNullOrWhiteSpace(cls.Id))
                    cls.Id = System.Guid.NewGuid().ToString();

                if (cls.Exams != null)
                {
                    foreach (var exam in cls.Exams)
                    {
                        if (exam != null && string.IsNullOrWhiteSpace(exam.Id))
                            exam.Id = System.Guid.NewGuid().ToString();
                    }
                }

                if (cls.Students != null)
                {
                    foreach (var student in cls.Students)
                    {
                        if (student != null && string.IsNullOrWhiteSpace(student.Id))
                            student.Id = System.Guid.NewGuid().ToString();
                    }
                }

                if (cls.Professors != null)
                {
                    foreach (var prof in cls.Professors)
                    {
                        if (prof != null && string.IsNullOrWhiteSpace(prof.Id))
                            prof.Id = System.Guid.NewGuid().ToString();
                    }
                }

                if (cls.Course != null && string.IsNullOrWhiteSpace(cls.Course.Id))
                {
                    cls.Course.Id = System.Guid.NewGuid().ToString();
                }
            }

            await _classRepository.CreateAsync(cls);
        }
    }
}

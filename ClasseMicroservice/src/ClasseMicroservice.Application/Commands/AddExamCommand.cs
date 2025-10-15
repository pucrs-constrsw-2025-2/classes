using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.Application.Commands
{
    public class AddExamCommand
    {
        public string ClassId { get; set; }
        public Exam Exam { get; set; }

        public AddExamCommand(string classId, Exam exam)
        {
            ClassId = classId;
            Exam = exam;
        }
    }
}
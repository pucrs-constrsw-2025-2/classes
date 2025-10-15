using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.Domain.Interfaces
{
    public interface IExamRepository
    {
        Task<List<Exam>> GetExamsByClassIdAsync(string classId);
        Task<Exam> GetExamByIdAsync(string examId);
        Task AddExamToClassAsync(string classId, Exam exam);
        Task UpdateExamAsync(string examId, Exam exam);
        Task DeleteExamAsync(string examId);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Queries
{
    public class GetExamsQueryHandler : IQueryHandler<GetExamsQuery, List<Exam>>
    {
        private readonly IExamRepository _examRepository;

        public GetExamsQueryHandler(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<List<Exam>> HandleAsync(GetExamsQuery query)
        {
            return await _examRepository.GetExamsByClassIdAsync(query.ClassId);
        }
    }
}
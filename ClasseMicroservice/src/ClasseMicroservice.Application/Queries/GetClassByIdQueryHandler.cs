using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Queries
{
    public class GetClassByIdQueryHandler : IQueryHandler<GetClassByIdQuery, Class>
    {
        private readonly IClassRepository _classRepository;
        public GetClassByIdQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }
        public async Task<Class> HandleAsync(GetClassByIdQuery query)
        {
            return await _classRepository.GetByIdAsync(query.Id);
        }
    }
}

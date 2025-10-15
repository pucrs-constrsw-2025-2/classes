using System.Threading.Tasks;
using ClasseMicroservice.API.Models;
using ClasseMicroservice.API.Data;

namespace ClasseMicroservice.API.Queries
{
    public class GetClassByIdQueryHandler : IQueryHandler<GetClassByIdQuery, Classe>
    {
        private readonly ClasseRepository _classeRepository;
        public GetClassByIdQueryHandler(ClasseRepository classeRepository)
        {
            _classeRepository = classeRepository;
        }
        public async Task<Classe> HandleAsync(GetClassByIdQuery query)
        {
            return await _classeRepository.GetById(query.Id);
        }
    }
}

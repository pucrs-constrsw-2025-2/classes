using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.Application.Queries
{
    public class GetClassesQueryHandler : IQueryHandler<GetClassesQuery, List<Class>>
    {
        private readonly IClassRepository _classRepository;

        public GetClassesQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<List<Class>> HandleAsync(GetClassesQuery query)
        {
            var classes = await _classRepository.GetAllAsync();
            
            // Apply filters
            if (query.Year.HasValue)
                classes = classes.Where(c => c.Year == query.Year).ToList();
            
            if (query.Semester.HasValue)
                classes = classes.Where(c => c.Semester == query.Semester).ToList();
            
            if (!string.IsNullOrEmpty(query.CourseId))
                classes = classes.Where(c => c.Course.Id == query.CourseId).ToList();

            // Apply pagination
            return classes
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .ToList();
        }
    }
}
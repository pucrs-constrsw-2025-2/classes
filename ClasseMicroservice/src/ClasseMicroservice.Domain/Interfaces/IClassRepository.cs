using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.Domain.Interfaces
{
    public interface IClassRepository
    {
        Task<List<Class>> GetAllAsync();
        Task<Class> GetByIdAsync(string id);
        Task CreateAsync(Class @class);
        Task UpdateAsync(string id, Class classIn);
        Task DeleteAsync(string id);
    }
}

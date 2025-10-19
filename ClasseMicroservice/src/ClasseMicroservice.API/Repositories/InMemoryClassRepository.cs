using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.API.Repositories
{
    public class InMemoryClassRepository : IClassRepository
    {
        private readonly List<Class> _store = new();

        public Task CreateAsync(Class @class)
        {
            if (string.IsNullOrEmpty(@class.Id))
                @class.Id = System.Guid.NewGuid().ToString();
            _store.Add(@class);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            var item = _store.FirstOrDefault(c => c.Id == id);
            if (item != null) _store.Remove(item);
            return Task.CompletedTask;
        }

        public Task<List<Class>> GetAllAsync()
        {
            return Task.FromResult(_store.ToList());
        }

        public Task<Class> GetByIdAsync(string id)
        {
            var item = _store.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(item);
        }

        public Task UpdateAsync(string id, Class classIn)
        {
            var idx = _store.FindIndex(c => c.Id == id);
            if (idx >= 0) _store[idx] = classIn;
            return Task.CompletedTask;
        }
    }
}

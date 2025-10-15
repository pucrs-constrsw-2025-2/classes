using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.API.Models;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ClasseMicroservice.API.Data
{
    public class ClassRepository : IClassRepository
    {
        private readonly IMongoCollection<Class> _classes;

        public ClassRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("ClassDb");
            _classes = database.GetCollection<Class>("Classes");
        }

        public async Task<List<Class>> GetAllAsync()
        {
            return await _classes.Find(c => true).ToListAsync();
        }

        public async Task<Class> GetByIdAsync(string id)
        {
            return await _classes.Find<Class>(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Class @class)
        {
            await _classes.InsertOneAsync(@class);
        }

        public async Task UpdateAsync(string id, Class classIn)
        {
            await _classes.ReplaceOneAsync(c => c.Id == id, classIn);
        }

        public async Task DeleteAsync(string id)
        {
            await _classes.DeleteOneAsync(c => c.Id == id);
        }
    }
}
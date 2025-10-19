using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClasseMicroservice.Domain.Entities;

namespace ClasseMicroservice.API.Data
{
    public class MigrationService
    {
        private readonly IMongoDatabase _database;

        public MigrationService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("ClassDb");
        }

        public async Task EnsureCollectionsAndIndexesAsync()
        {
            // Ensure collections
            await EnsureCollectionExistsAsync<Class>("Classes");
            await EnsureCollectionExistsAsync<Exam>("Exams");
            await EnsureCollectionExistsAsync<Student>("Students");
            await EnsureCollectionExistsAsync<Professor>("Professors");
            await EnsureCollectionExistsAsync<Course>("Courses");

            // Basic indexes
            await EnsureIndexAsync<Class>("Classes", Builders<Class>.IndexKeys.Ascending(c => c.Id));
            await EnsureIndexAsync<Exam>("Exams", Builders<Exam>.IndexKeys.Ascending(e => e.Id));
            await EnsureIndexAsync<Student>("Students", Builders<Student>.IndexKeys.Ascending(s => s.Id));
            await EnsureIndexAsync<Professor>("Professors", Builders<Professor>.IndexKeys.Ascending(p => p.Id));
            await EnsureIndexAsync<Course>("Courses", Builders<Course>.IndexKeys.Ascending(c => c.Id));
        }

        private async Task EnsureCollectionExistsAsync<T>(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            if (!await collections.AnyAsync())
            {
                await _database.CreateCollectionAsync(collectionName);
            }
        }

        private async Task EnsureIndexAsync<T>(string collectionName, IndexKeysDefinition<T> indexKeys)
        {
            var collection = _database.GetCollection<T>(collectionName);
            var indexModel = new CreateIndexModel<T>(indexKeys);
            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }
}

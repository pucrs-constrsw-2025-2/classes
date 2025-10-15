using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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
            await EnsureCollectionExistsAsync<Models.Class>("Classes");
            await EnsureCollectionExistsAsync<Models.Exam>("Exams");
            await EnsureCollectionExistsAsync<Models.Student>("Students");
            await EnsureCollectionExistsAsync<Models.Professor>("Professors");
            await EnsureCollectionExistsAsync<Models.Course>("Courses");

            // Basic indexes
            await EnsureIndexAsync<Models.Class>("Classes", Builders<Models.Class>.IndexKeys.Ascending(c => c.Id));
            await EnsureIndexAsync<Models.Exam>("Exams", Builders<Models.Exam>.IndexKeys.Ascending(e => e.Id));
            await EnsureIndexAsync<Models.Student>("Students", Builders<Models.Student>.IndexKeys.Ascending(s => s.Id));
            await EnsureIndexAsync<Models.Professor>("Professors", Builders<Models.Professor>.IndexKeys.Ascending(p => p.Id));
            await EnsureIndexAsync<Models.Course>("Courses", Builders<Models.Course>.IndexKeys.Ascending(c => c.Id));
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

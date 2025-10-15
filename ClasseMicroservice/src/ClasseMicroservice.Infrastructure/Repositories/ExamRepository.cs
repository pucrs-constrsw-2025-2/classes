using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ClasseMicroservice.Infrastructure.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly IMongoCollection<Exam> _exams;
        private readonly IMongoCollection<Class> _classes;

        public ExamRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("ClassDb");
            _exams = database.GetCollection<Exam>("Exams");
            _classes = database.GetCollection<Class>("Classes");
        }

        public async Task<List<Exam>> GetExamsByClassIdAsync(string classId)
        {
            var @class = await _classes.Find(c => c.Id == classId).FirstOrDefaultAsync();
            if (@class == null) return new List<Exam>();
            return @class.Exams ?? new List<Exam>();
        }

        public async Task<Exam> GetExamByIdAsync(string examId)
        {
            return await _exams.Find(e => e.Id == examId).FirstOrDefaultAsync();
        }

        public async Task AddExamToClassAsync(string classId, Exam exam)
        {
            await _exams.InsertOneAsync(exam);
            var update = Builders<Class>.Update.Push(c => c.Exams, exam);
            await _classes.UpdateOneAsync(c => c.Id == classId, update);
        }

        public async Task UpdateExamAsync(string examId, Exam exam)
        {
            await _exams.ReplaceOneAsync(e => e.Id == examId, exam);
        }

        public async Task DeleteExamAsync(string examId)
        {
            await _exams.DeleteOneAsync(e => e.Id == examId);
            var update = Builders<Class>.Update.PullFilter(c => c.Exams, e => e.Id == examId);
            await _classes.UpdateManyAsync(FilterDefinition<Class>.Empty, update);
        }
    }
}
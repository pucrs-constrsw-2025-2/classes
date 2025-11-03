using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace ClasseMicroservice.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var conn = _configuration.GetConnectionString("MongoDb");
                if (string.IsNullOrEmpty(conn))
                {
                    // fallback to environment variables
                    var host = _configuration["MONGODB_HOST"] ?? _configuration["MONGODB_INTERNAL_HOST"] ?? "mongodb";
                    var port = _configuration["MONGODB_PORT"] ?? _configuration["MONGODB_INTERNAL_PORT"] ?? "27017";
                    var user = _configuration["MONGODB_USERNAME"];
                    var pass = _configuration["MONGODB_PASSWORD"];
                    var db = _configuration["MONGODB_DATABASE"] ?? _configuration["MONGODB_DB"] ?? "classes";

                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
                        conn = $"mongodb://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@{host}:{port}/{db}";
                    else
                        conn = $"mongodb://{host}:{port}/{db}";
                }

                var client = new MongoClient(conn);

                var dbName = _configuration.GetSection("MongoDbSettings").GetValue<string>("DatabaseName")
                             ?? _configuration["MONGODB_DATABASE"]
                             ?? _configuration["MONGODB_DB"]
                             ?? "classes";

                var database = client.GetDatabase(dbName);

                // ping
                var command = new BsonDocument("ping", 1);
                await database.RunCommandAsync<BsonDocument>(command);

                return Ok(new { status = "ok", database = dbName });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "unavailable", error = ex.Message });
            }
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;

namespace CarLotFunctions
{
    public static class GetCars
    {
        [FunctionName("GetCars")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                var driver = new MongoClient(config["mongoConnectionString"]);
                var db = driver.GetDatabase("jobfit-carlot-db");
                var collection = db.GetCollection<CarModel>("jobfit-carlot-db");
                var results = collection.Find(new BsonDocument()).ToList();

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {

                //return new BadRequestObjectResult("Could not parse data");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

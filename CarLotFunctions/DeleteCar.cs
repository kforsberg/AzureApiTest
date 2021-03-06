using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace CarLotFunctions
{
    public static class DeleteCar
    {
        [FunctionName("DeleteCar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "cars/{id}")] HttpRequest req,
            string id, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            log.LogError("Hello");

            try
            {
                var driver = new MongoClient(config["mongoConnectionString"]);
                var db = driver.GetDatabase("jobfit-carlot-db");
                var collection = db.GetCollection<CarModel>("jobfit-carlot-db");
                await collection.DeleteOneAsync(car => car.Id == new BsonObjectId(new ObjectId(id)));

                return new OkObjectResult("Record Deleted!");
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("An unexpected error occurred");
            }
        }
    }
}

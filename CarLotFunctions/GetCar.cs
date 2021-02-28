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
    public static class GetCar
    {
        [FunctionName("GetCar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCar/{id}")] HttpRequest req,
            string id, ExecutionContext context, ILogger log)
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
                var filter = Builders<CarModel>.Filter.Eq("id", id);
                var car = collection.Find(filter).FirstOrDefault();

                return new OkObjectResult(car);
            }
            catch (Exception ex)
            {

                //return new BadRequestObjectResult("Could not parse data");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

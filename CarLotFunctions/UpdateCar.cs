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
    public static class UpdateCar
    {
        [FunctionName("UpdateCar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "cars/{id}")] HttpRequest req,
            string id, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
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
                var updateFilter = Builders<CarModel>.Filter.Eq("id", new BsonObjectId(new ObjectId(id)));
                var car = collection.Find(car => car.Id == new BsonObjectId(new ObjectId(id))).FirstOrDefault();
                var newCar = CarModel.Build(data);
                if (car == null)
                {
                    return new NotFoundObjectResult("No Content");
                }

                newCar.Id = car.Id;
                collection.ReplaceOne(Builders<CarModel>.Filter.Eq(car => car.Id, new BsonObjectId(new ObjectId(id))), newCar);

                return new OkObjectResult(newCar);
            }
            catch (Exception ex)
            {
                //return new BadRequestObjectResult("An unexpected error occurred");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

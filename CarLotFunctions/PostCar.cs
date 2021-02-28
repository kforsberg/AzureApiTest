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

namespace CarLotFunctions
{
    public static class PostCar
    {
        [FunctionName("PostCar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ExecutionContext context, ILogger log)
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
                var car = CarModel.Build(data);
                var db = driver.GetDatabase("jobfit-carlot-db");
                var collection = db.GetCollection<CarModel>("jobfit-carlot-db");
                collection.InsertOne(car);

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

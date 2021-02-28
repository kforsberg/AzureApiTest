
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace functions
{
    public static class DeleteCar
    {
        [FunctionName("DeleteCar")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{id:string}")] HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                string id = req.Query["id"];
                var driver = new MongoClient(config["mongoConnectionString"]);
                var db = driver.GetDatabase("jobfit-carlot-db");
                var collection = db.GetCollection<CarModel>("jobfit-carlot-db");
                var deleteFilter = Builders<CarModel>.Filter.Eq("id", id);
                collection.DeleteOne(deleteFilter);

                return new OkObjectResult("Record Deleted");
            }
            catch (Exception ex)
            {

                //return new BadRequestObjectResult("Could not parse data");
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

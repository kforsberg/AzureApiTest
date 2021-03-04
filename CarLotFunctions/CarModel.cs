using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarLotFunctions
{
    public class CarModel
    {
        public BsonObjectId Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int? Year { get; set; }
        public string Color { get; set; }
        public bool IsNew { get; set; }
        public int? Cost { get; set; }
        public int? Mileage { get; set; }

        public static CarModel Build(dynamic data)
        {
            return new CarModel
            {
                Make = data?.make,
                Model = data?.model,
                Year = data?.year,
                Color = data?.color,
                IsNew = data?.isNew ?? false,
                Cost = data?.cost,
                Mileage = data?.mileage
            };
        }
    }
}

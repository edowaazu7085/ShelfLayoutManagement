using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Lane
    {
        [BsonRepresentation(BsonType.Int32)]
        public int Number { get; set; }

        public string JanCode { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int Quantity { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int PositionX { get; set; }
    }
}
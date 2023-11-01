using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Position
    {
        [BsonRepresentation(BsonType.Int32)]
        public int X { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int Y { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int Z { get; set; }
        
    }
}

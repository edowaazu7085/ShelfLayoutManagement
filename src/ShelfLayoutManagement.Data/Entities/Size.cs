using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Size
    {
        [BsonRepresentation(BsonType.Int32)]
        public int? Width { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int? Depth { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int Height { get; set; }
    }
}

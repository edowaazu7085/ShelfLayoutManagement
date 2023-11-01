using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string JanCode { get; set; }
        public string Name { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public double X { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public double Y { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public double Z { get; set; }
        [BsonElement("ImageURL")]
        public string ImageUrl { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int Size { get; set; }
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("TimeStamp")]
        public long Timestamp { get; set; }
        public string Shape { get; set; }
    }
}

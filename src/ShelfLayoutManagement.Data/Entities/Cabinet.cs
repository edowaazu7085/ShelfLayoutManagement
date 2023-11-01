using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Cabinet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int Number { get; set; }

        public IList<Row> Rows { get; set; }

        public Position Position { get; set; }

        public Size Size { get; set; }
    }
}

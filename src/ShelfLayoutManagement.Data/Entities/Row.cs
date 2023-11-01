using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShelfLayoutManagement.Data.Entities
{
    public class Row
    {
        [BsonRepresentation(BsonType.Int32)]
        public int Number { get; set; }

        public IList<Lane> Lanes { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int PositionZ { get; set; }

        public Size Size { get; set; }
    }
}

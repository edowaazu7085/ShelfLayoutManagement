using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Data.Entities;

namespace ShelfLayoutManagement.Data.Converters
{
    public class LaneConverter : IConverter<Lane, ShelfLayoutManagement.Common.Shelf.Lane>
    {
        public Common.Shelf.Lane Convert(Lane source)
        {
            return new ShelfLayoutManagement.Common.Shelf.Lane
            {
                Number = source.Number,
                JanCode = source.JanCode,
                Quantity = source.Quantity,
                PositionX = source.PositionX
            };
        }

        public Lane Convert(Common.Shelf.Lane destination)
        {
            return new Lane
            {
                Number = destination.Number,
                JanCode = destination.JanCode,
                Quantity = destination.Quantity,
                PositionX = destination.PositionX
            };
        }
    }
}

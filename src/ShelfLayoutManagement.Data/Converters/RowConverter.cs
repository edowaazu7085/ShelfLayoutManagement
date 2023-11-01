using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Data.Entities;

namespace ShelfLayoutManagement.Data.Converters
{
    public class RowConverter : IConverter<Row, ShelfLayoutManagement.Common.Shelf.Row>
    {
        public Common.Shelf.Row Convert(Row source)
        {
            var newRow = new ShelfLayoutManagement.Common.Shelf.Row
            {
                Number = source.Number,
                PositionZ = source.PositionZ,
                Size = new Common.Shelf.Row.Types.Size
                {
                    Height = source.Size.Height
                }
            };
            var newLanes = source.Lanes.Select(lane => new ShelfLayoutManagement.Common.Shelf.Lane
            {
                Number = lane.Number,
                JanCode = lane.JanCode,
                Quantity = lane.Quantity,
                PositionX = lane.PositionX
            }).ToList();
            newRow.Lanes.AddRange(newLanes);

            return newRow;
        }

        public Row Convert(Common.Shelf.Row destination)
        {
            var newRow = new Row
            {
                Number = destination.Number,
                PositionZ = destination.PositionZ,
                Size = new Size
                {
                    Height = destination.Size.Height
                }
            };
            var newLanes = destination.Lanes.Select(lane => new Lane
            {
                Number = lane.Number,
                JanCode = lane.JanCode,
                Quantity = lane.Quantity,
                PositionX = lane.PositionX
            }).ToList();
            newRow.Lanes = newLanes;

            return newRow;
        }
    }
}

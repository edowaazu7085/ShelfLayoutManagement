using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Data.Entities;
using Cabinet = ShelfLayoutManagement.Common.Shelf.Cabinet;

namespace ShelfLayoutManagement.Data.Converters
{
    public class CabinetConverter : IConverter<Entities.Cabinet, ShelfLayoutManagement.Common.Shelf.Cabinet>
    {
        public Cabinet Convert(Entities.Cabinet source)
        {
            var newCabinet = new ShelfLayoutManagement.Common.Shelf.Cabinet
            {
                Number = source.Number,
                Position = new Cabinet.Types.Position
                {
                    X = source.Position.X,
                    Y = source.Position.Y,
                    Z = source.Position.Z
                },
                Size = new Cabinet.Types.Size
                {
                    Width = source.Size.Width ?? 0,
                    Depth = source.Size.Depth ?? 0,
                    Height = source.Size.Height
                }
            };
            if (source.Rows.Any())
            {
                var newRows = new List<ShelfLayoutManagement.Common.Shelf.Row>();
                foreach (var row in source.Rows)
                {
                    var newRow = new ShelfLayoutManagement.Common.Shelf.Row
                    {
                        Number = row.Number,
                        PositionZ = row.PositionZ,
                        Size = new Common.Shelf.Row.Types.Size
                        {
                            Height = row.Size.Height
                        }
                    };
                    var newLanes = row.Lanes.Select(lane => new ShelfLayoutManagement.Common.Shelf.Lane
                    {
                        Number = lane.Number,
                        JanCode = lane.JanCode,
                        Quantity = lane.Quantity,
                        PositionX = lane.PositionX
                    }).ToList();
                    newRow.Lanes.AddRange(newLanes);
                    newRows.Add(newRow);
                }

                newCabinet.Rows.AddRange(newRows);
            }

            return newCabinet;
        }

        public Entities.Cabinet Convert(Cabinet destination)
        {
            var newCabinet = new Entities.Cabinet
            {
                Number = destination.Number,
                Position = new Position
                {
                    X = destination.Position.X,
                    Y = destination.Position.Y,
                    Z = destination.Position.Z
                },
                Size = new Size
                {
                    Width = destination.Size.Width,
                    Depth = destination.Size.Depth,
                    Height = destination.Size.Height
                }
            };
            if (destination.Rows != null && destination.Rows.Any())
            {
                var newRows = new List<Row>();
                foreach (var row in destination.Rows)
                {
                    var newRow = new Row
                    {
                        Number = row.Number,
                        PositionZ = row.PositionZ,
                        Size = new Size
                        {
                            Height = row.Size.Height
                        }
                    };
                    var newLanes = row.Lanes.Select(lane => new Lane
                    {
                        Number = lane.Number,
                        JanCode = lane.JanCode,
                        Quantity = lane.Quantity,
                        PositionX = lane.PositionX
                    }).ToList();
                    newRow.Lanes = newLanes;
                    newRows.Add(newRow);
                }

                newCabinet.Rows = newRows;
            }

            return newCabinet;
        }
    }
}

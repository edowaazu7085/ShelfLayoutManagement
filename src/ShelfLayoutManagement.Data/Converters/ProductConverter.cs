using ShelfLayoutManagement.Common.Interfaces;

using Product = ShelfLayoutManagement.Data.Entities.Product;

namespace ShelfLayoutManagement.Data.Converters
{
    public class ProductConverter : IConverter<ShelfLayoutManagement.Data.Entities.Product, ShelfLayoutManagement.Common.StockKeepingUnit.Product>
    {
        public Common.StockKeepingUnit.Product Convert(Product source)
        {
            var newProduct = new Common.StockKeepingUnit.Product
            {
                Shape = source.Shape,
                ImageUrl = source.ImageUrl,
                JanCode = source.JanCode,
                Name = source.Name,
                Size = source.Size,
                Timestamp = source.Timestamp,
                X = source.X,
                Y = source.Y,
                Z = source.Z
            };

            return newProduct;
        }

        public Product Convert(Common.StockKeepingUnit.Product destination)
        {
            var newProduct = new Product
            {
                Shape = destination.Shape,
                ImageUrl = destination.ImageUrl,
                JanCode = destination.JanCode,
                Name = destination.Name,
                Size = destination.Size,
                Timestamp = destination.Timestamp,
                X = destination.X,
                Y = destination.Y,
                Z = destination.Z
            };

            return newProduct;
        }
    }
}

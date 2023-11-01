using Grpc.Core;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Common.StockKeepingUnit;
using ShelfLayoutManagement.Data.Repositories;

namespace ShelfLayoutManagement.Server.Services
{
    [Authorize]
    public class StockKeepingUnitService : Products.ProductsBase
    {
        private readonly ILogger _logger;
        private readonly IStockKeepingUnitRepository _stockKeepingUnitRepository;
        private readonly IConverter<Data.Entities.Product, Common.StockKeepingUnit.Product> _productConverter;

        public StockKeepingUnitService(ILogger<ShelvesService> logger, IStockKeepingUnitRepository stockKeepingUnitRepository,
            IConverter<Data.Entities.Product, Common.StockKeepingUnit.Product> productConverter)
        {
            _logger = logger;
            _stockKeepingUnitRepository = stockKeepingUnitRepository;
            _productConverter = productConverter;
        }

        public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
        {
            var page = request.PageSize * request.Page;
            var products = await _stockKeepingUnitRepository.ListProductsAsync(page, request.PageSize, context.CancellationToken);
            var listToReturn = products.Select(x => _productConverter.Convert(x)).ToList();

            var response = new ListProductsResponse();
            response.Products.AddRange(listToReturn);

            return response;
        }

        public override async Task StreamProducts(StreamProductsRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            var products = await _stockKeepingUnitRepository.ListProductsAsync(token: context.CancellationToken);

            foreach (var product in products)
            {
                var productProto = _productConverter.Convert(product);
                await responseStream.WriteAsync(productProto, context.CancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }

        public override async Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
        {
            var convertedProduct = _productConverter.Convert(request.Product);

            await _stockKeepingUnitRepository.CreateProductAsync(convertedProduct, context.CancellationToken);

            return request.Product;
        }

        public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _stockKeepingUnitRepository.GetProductAsync(request.JanCode,request.Name, context.CancellationToken);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            var productProto = _productConverter.Convert(product);
            return productProto;
        }

        public override async Task<Product> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var convertedProduct = _productConverter.Convert(request.Product);

            var updateResult = await _stockKeepingUnitRepository.UpdateProductAsync(convertedProduct, context.CancellationToken);
            if (updateResult.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            return request.Product;
        }

        public override async Task<Empty> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var deleteResult = await _stockKeepingUnitRepository.RemoveProductAsync(request.JanCode, context.CancellationToken);
            if (deleteResult.DeletedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            return new Empty();
        }
    }
}

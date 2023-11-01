using System.Linq.Expressions;
using MongoDB.Driver;

using ShelfLayoutManagement.Common.Helpers;
using ShelfLayoutManagement.Data.Entities;
using ShelfLayoutManagement.Data.Models;

namespace ShelfLayoutManagement.Data.Repositories
{
    public interface IStockKeepingUnitRepository
    {
        Task<List<Entities.Product>> ListProductsAsync(int? skip = null, int? limit = null, CancellationToken token = default);
        Task<Entities.Product?> GetProductAsync(string janCode, string name, CancellationToken token = default);
        Task CreateProductAsync(Entities.Product newProduct, CancellationToken token = default);
        Task CreateProductAsync(IEnumerable<Entities.Product> newProductList, CancellationToken token = default);
        Task<ReplaceOneResult> UpdateProductAsync(Entities.Product updatedCabinet, CancellationToken token = default);
        Task<DeleteResult> RemoveProductAsync(string janCode, CancellationToken token = default);
    }

    public class StockKeepingUnitRepository : IStockKeepingUnitRepository
    {
        private readonly IMongoCollection<Entities.Product> _productCollection;

        public StockKeepingUnitRepository(IMongoClient mongoClient, IStockKeepingUnitCollectionSettings configuration)
        {
            var mongoDatabase = mongoClient.GetDatabase(configuration.DatabaseName);
            _productCollection = mongoDatabase.GetCollection<Entities.Product>(configuration.CollectionName);
        }
        
        public async Task<List<Entities.Product>> ListProductsAsync(int? skip = null, int? limit = null, CancellationToken token = default)
        {
            var query = _productCollection.Find(_ => true);
            if (skip != null && limit != null)
            {
                query = query.Skip(skip).Limit(limit);
            }

            return await query.ToListAsync(token);
        }

        public async Task<Entities.Product?> GetProductAsync(string janCode, string name, CancellationToken token = default)
        {
            var predicate = PredicateBuilder.False<Product>();
            if (!string.IsNullOrEmpty(janCode))
            {
                predicate = predicate.Or(p => p.JanCode == janCode);
            }
            if (!string.IsNullOrEmpty(name))
            {
                predicate = predicate.Or(p => p.Name.Contains(name));
            }

            return await _productCollection.Find(predicate).FirstOrDefaultAsync(token);
        }

        public async Task CreateProductAsync(Entities.Product newProduct, CancellationToken token = default)
        {
            await _productCollection.InsertOneAsync(newProduct, cancellationToken: token);
        }

        public async Task CreateProductAsync(IEnumerable<Entities.Product> newProductList, CancellationToken token = default)
        {
            await _productCollection.InsertManyAsync(newProductList, cancellationToken: token);
        }

        public async Task<ReplaceOneResult> UpdateProductAsync(Entities.Product updatedCabinet, CancellationToken token = default)
        {
            return await _productCollection.ReplaceOneAsync(x => x.JanCode == updatedCabinet.JanCode,updatedCabinet, cancellationToken: token);
        }

        public async Task<DeleteResult> RemoveProductAsync(string janCode, CancellationToken token = default)
        {
            return await _productCollection.DeleteOneAsync(x => x.JanCode == janCode, token);
        }

        
    }
}
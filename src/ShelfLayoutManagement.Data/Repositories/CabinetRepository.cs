using MongoDB.Bson;
using MongoDB.Driver;
using ShelfLayoutManagement.Data.Entities;
using ShelfLayoutManagement.Data.Exceptions;
using ShelfLayoutManagement.Data.Models;

namespace ShelfLayoutManagement.Data.Repositories
{
    public interface ICabinetRepository
    {
        Task<List<Cabinet>> ListCabinetsAsync(int? skip = null, int? limit = null, CancellationToken token = default);
        Task CreateCabinetAsync(Cabinet newCabinet, CancellationToken token = default);
        Task CreateCabinetAsync(IEnumerable<Cabinet> newCabinetList, CancellationToken token = default);
        Task<int> GetNextCabinetNumberAsync(CancellationToken token = default);
        Task<Cabinet?> GetCabinetAsync(int number, CancellationToken token = default);
        Task<IEnumerable<Cabinet>> GetCabinetByJanCodeAsync(string janCode, CancellationToken token = default);
        Task<ReplaceOneResult> UpdateCabinetAsync(Cabinet updatedCabinet, CancellationToken token = default);
        Task<DeleteResult> DeleteCabinetAsync(int number, CancellationToken token = default);
        Task<UpdateResult> CreateRowAsync(int cabinetNumber, Row newRow, CancellationToken token = default);
        Task<Row> GetRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default);
        Task<int> GetNextRowNumberInCabinetAsync(int cabinetNumber, CancellationToken token = default);
        Task<UpdateResult> UpdateRowAsync(int cabinetNumber, int rowNumber, Row updatedRow, CancellationToken token = default);
        Task<UpdateResult> DeleteRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default);
        Task<UpdateResult> CreateLaneAsync(int cabinetNumber, int rowNumber, Lane newLane, CancellationToken token = default);
        Task<Lane> GetLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, CancellationToken token = default);
        Task<int> GetNextLaneNumberInRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default);
        Task<UpdateResult> UpdateLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, string newJanCode,
            int newQuantity, CancellationToken token = default);
        Task<UpdateResult> DeleteLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, CancellationToken token = default);
    }

    public class CabinetRepository : ICabinetRepository
    {
        private readonly IMongoCollection<Cabinet> _cabinetCollection;

        public CabinetRepository(IMongoClient mongoClient, ICabinetCollectionSettings configuration)
        {
            var mongoDatabase = mongoClient.GetDatabase(configuration.DatabaseName);
            _cabinetCollection = mongoDatabase.GetCollection<Cabinet>(configuration.CollectionName);
        }
        
        public async Task<List<Cabinet>> ListCabinetsAsync(int? skip = null, int? limit = null, CancellationToken token = default)
        {
            var query = _cabinetCollection.Find(_ => true);
            if (skip != null && limit != null)
            {
                query = query.Skip(skip).Limit(limit);
            }

            return await query.ToListAsync(token);
        }

        public async Task CreateCabinetAsync(Cabinet newCabinet, CancellationToken token = default)
        {
            await _cabinetCollection.InsertOneAsync(newCabinet, cancellationToken: token);
        }

        public async Task CreateCabinetAsync(IEnumerable<Cabinet> newCabinetList, CancellationToken token = default)
        {
            await _cabinetCollection.InsertManyAsync(newCabinetList, cancellationToken: token);
        }

        public async Task<int> GetNextCabinetNumberAsync(CancellationToken token = default)
        {
            var latestCabinet = await _cabinetCollection
                .Find(new BsonDocument())
                .SortByDescending(c => c.Number)
                .Limit(1)
                .FirstOrDefaultAsync(token);

            if (latestCabinet != null)
            {
                return latestCabinet.Number + 1;
            }

            return 1;
        }

        public async Task<Cabinet?> GetCabinetAsync(int number, CancellationToken token = default)
        {
            return await _cabinetCollection.Find(x => x.Number == number).FirstOrDefaultAsync(token);
        }

        public async Task<IEnumerable<Cabinet>> GetCabinetByJanCodeAsync(string janCode, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.ElemMatch(
                cabinet => cabinet.Rows,
                row => row.Lanes.Any(lane => lane.JanCode == janCode)
            );

            return await _cabinetCollection.Find(filter).ToListAsync(token);
        }
        
        public async Task<ReplaceOneResult> UpdateCabinetAsync(Cabinet updatedCabinet, CancellationToken token = default)
        {
            return await _cabinetCollection.ReplaceOneAsync(x => x.Number == updatedCabinet.Number,updatedCabinet, cancellationToken: token);
        }

        public async Task<DeleteResult> DeleteCabinetAsync(int number, CancellationToken token = default)
        {
            return await _cabinetCollection.DeleteOneAsync(x => x.Number == number, token);
        }

        public async Task<UpdateResult> CreateRowAsync(int cabinetNumber, Row newRow, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber);
            var update = Builders<Cabinet>.Update.Push(c => c.Rows, newRow);

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }

        public async Task<Row> GetRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber) &
                         Builders<Cabinet>.Filter.Eq(c => c.Rows[rowNumber - 1].Number, rowNumber);

            var projection = Builders<Cabinet>.Projection.Include(c => c.Rows[-1]);

            var lane = await _cabinetCollection.Find(filter).Project<Row>(projection).FirstOrDefaultAsync(token);

            return lane;
        }

        public async Task<int> GetNextRowNumberInCabinetAsync(int cabinetNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber);
            var cabinet = await _cabinetCollection.Find(filter).FirstOrDefaultAsync(token);
            if (cabinet == null)
            {
                throw new DataNotFoundException("The cabinet number does not exist");
            }

            int nextRowNumber = 1;
            if (cabinet.Rows.Any())
            {
                // Determine the next available row number by finding the maximum row number in the existing rows.
                nextRowNumber = cabinet.Rows.Max(r => r.Number) + 1;
            }

            return nextRowNumber;
        }

        public async Task<UpdateResult> UpdateRowAsync(int cabinetNumber, int rowNumber, Row updatedRow, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.And(
                Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber),
                Builders<Cabinet>.Filter.ElemMatch(c => c.Rows, r => r.Number == rowNumber)
            );

            var update = Builders<Cabinet>.Update
                .Set(c => c.Rows[-1], updatedRow); // -1 refers to the last matched element

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }

        public async Task<UpdateResult> DeleteRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.And(
                Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber),
                Builders<Cabinet>.Filter.Eq("Rows.Number", rowNumber)
            );

            var update = Builders<Cabinet>.Update.PullFilter(c => c.Rows, r => r.Number == rowNumber);

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }

        public async Task<UpdateResult> CreateLaneAsync(int cabinetNumber, int rowNumber, Lane newLane, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber);
            var update = Builders<Cabinet>.Update.Push($"Rows.{rowNumber - 1}.Lanes", newLane);

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }

        public async Task<Lane> GetLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber) &
                         Builders<Cabinet>.Filter.Eq(c => c.Rows[rowNumber - 1].Lanes[laneNumber - 1].Number, laneNumber);

            var projection = Builders<Cabinet>.Projection.Include(c => c.Rows[-1].Lanes[-1]); // Include only the necessary fields

            var lane = await _cabinetCollection.Find(filter).Project<Lane>(projection).FirstOrDefaultAsync(token);

            return lane;
        }

        public async Task<int> GetNextLaneNumberInRowAsync(int cabinetNumber, int rowNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.And(
                Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber),
                Builders<Cabinet>.Filter.ElemMatch(c => c.Rows, r => r.Number == rowNumber)
            );

            var cabinet = await _cabinetCollection.Find(filter).FirstOrDefaultAsync(token);
            if (cabinet == null)
            {
                throw new DataNotFoundException("The cabinet number does not exist");
            }
            if (!cabinet.Rows.Any())
            {
                throw new DataNotFoundException("The cabinet number does not contain rows");
            }

            var row = cabinet.Rows.First(r => r.Number == rowNumber);
            int nextLaneNumber = 1;

            if (row.Lanes.Any())
            {
                // Determine the next available lane number by finding the maximum lane number in the existing lanes.
                nextLaneNumber = row.Lanes.Max(l => l.Number) + 1;
            }

            return nextLaneNumber;
        }

        public async Task<UpdateResult> UpdateLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, string newJanCode, int newQuantity, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber);
            var update = Builders<Cabinet>.Update
                .Set($"Rows.{rowNumber - 1}.Lanes.{laneNumber - 1}.JanCode", newJanCode)
                .Set($"Rows.{rowNumber - 1}.Lanes.{laneNumber - 1}.Quantity", newQuantity);

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken: token);
        }

        public async Task<UpdateResult> DeleteLaneAsync(int cabinetNumber, int rowNumber, int laneNumber, CancellationToken token = default)
        {
            var filter = Builders<Cabinet>.Filter.Eq(c => c.Number, cabinetNumber);
            var update = Builders<Cabinet>.Update.PullFilter($"Rows.{rowNumber - 1}.Lanes",
                Builders<Lane>.Filter.Eq(l => l.Number, laneNumber));

            return await _cabinetCollection.UpdateOneAsync(filter, update, cancellationToken:token);
        }
    }
}
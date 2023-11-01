using Grpc.Core;

using Microsoft.AspNetCore.Authorization;

using MongoDB.Driver;

using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Common.Shelf;
using ShelfLayoutManagement.Data.Repositories;

using Cabinet = ShelfLayoutManagement.Common.Shelf.Cabinet;
using Lane = ShelfLayoutManagement.Common.Shelf.Lane;
using Row = ShelfLayoutManagement.Common.Shelf.Row;

namespace ShelfLayoutManagement.Server.Services
{
    public class ShelvesService : Shelves.ShelvesBase
    {
        private readonly ILogger _logger;
        private readonly ICabinetRepository _cabinetRepository;
        private readonly IMongoClient _mongoClient;
        private readonly IConverter<Data.Entities.Cabinet, Cabinet> _cabinetConverter;
        private readonly IConverter<Data.Entities.Row, Row> _rowConverter;
        private readonly IConverter<Data.Entities.Lane, Lane> _laneConverter;

        public ShelvesService(IMongoClient mongoClient, ILogger<ShelvesService> logger, ICabinetRepository cabinetRepository, 
            IConverter<Data.Entities.Cabinet, Cabinet> cabinetConverter,
            IConverter<Data.Entities.Row, Row> rowConverter,
            IConverter<Data.Entities.Lane, Lane> laneConverter)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _cabinetRepository = cabinetRepository;
            _cabinetConverter = cabinetConverter;
            _rowConverter = rowConverter;
            _laneConverter = laneConverter;
        }

        public override async Task<ListCabinetsResponse> ListCabinets(ListCabinetsRequest request, ServerCallContext context)
        {
            var page = request.PageSize * request.Page;
            var cabinets = await _cabinetRepository.ListCabinetsAsync(page, request.PageSize, context.CancellationToken);
            var listToReturn = cabinets.Select(x => _cabinetConverter.Convert(x)).ToList();

            var response = new ListCabinetsResponse();
            response.Cabinets.AddRange(listToReturn);

            return response;
        }

        public override async Task StreamCabinets(StreamCabinetsRequest request, IServerStreamWriter<Cabinet> responseStream, ServerCallContext context)
        {
            var cabinets = await _cabinetRepository.ListCabinetsAsync(token: context.CancellationToken);

            foreach (var cabinet in cabinets)
            {
                var cabinetProto = _cabinetConverter.Convert(cabinet);

                await responseStream.WriteAsync(cabinetProto, context.CancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }

        public override async Task<Cabinet> CreateCabinet(CreateCabinetRequest request, ServerCallContext context)
        {
            var nextCabinetNumber = await _cabinetRepository.GetNextCabinetNumberAsync();
            request.Cabinet.Number = nextCabinetNumber;

            var newCabinet = _cabinetConverter.Convert(request.Cabinet);

            await _cabinetRepository.CreateCabinetAsync(newCabinet, context.CancellationToken);
            
            return request.Cabinet;
        }

        public override async Task<Cabinet> GetCabinet(GetCabinetRequest request, ServerCallContext context)
        {
            var cabinet = await _cabinetRepository.GetCabinetAsync(request.CabinetNumber, context.CancellationToken);
            if (cabinet == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Cabinet not found"));
            }

            var cabinetProto = _cabinetConverter.Convert(cabinet);
            return cabinetProto;
        }

        public override async Task<Cabinet> UpdateCabinet(UpdateCabinetRequest request, ServerCallContext context)
        {
            var cabinetEntity = _cabinetConverter.Convert(request.Cabinet);

            var updateResult = await _cabinetRepository.UpdateCabinetAsync(cabinetEntity, context.CancellationToken);
            if (updateResult.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Cabinet not found"));
            }

            return request.Cabinet;
        }

        public override async Task<Empty> DeleteCabinet(DeleteCabinetRequest request, ServerCallContext context)
        {
            var deleteResult = await _cabinetRepository.DeleteCabinetAsync(request.Number, context.CancellationToken);
            if (deleteResult.DeletedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Cabinet not found"));
            }

            return new Empty();
        }
        
        public override async Task<Row> CreateRow(CreateRowRequest request, ServerCallContext context)
        {
            var nextCabinetNumber = await _cabinetRepository.GetNextRowNumberInCabinetAsync(request.CabinetNumber, context.CancellationToken);
            request.Row.Number = nextCabinetNumber;

            var convertedRow = _rowConverter.Convert(request.Row);

            var result = await _cabinetRepository.CreateRowAsync(request.CabinetNumber, convertedRow, context.CancellationToken);
            if (result.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.Aborted, "Row was not created"));
            }

            return request.Row;
        }

        public override async Task<Row> GetRow(GetRowRequest request, ServerCallContext context)
        {
            var cabinet = await _cabinetRepository.GetRowAsync(request.CabinetNumber, request.RowNumber, context.CancellationToken);
            if (cabinet == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Row not found"));
            }

            var cabinetProto = _rowConverter.Convert(cabinet);
            return cabinetProto;
        }

        public override async Task<Row> UpdateRow(UpdateRowRequest request, ServerCallContext context)
        {
            var cabinetEntity = _rowConverter.Convert(request.Row);
            var cabinet = await _cabinetRepository.UpdateRowAsync(request.CabinetNumber, request.Row.Number, cabinetEntity, context.CancellationToken);
            if (cabinet == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Lane not found"));
            }

            return request.Row;
        }

        public override async Task<Empty> DeleteRow(DeleteRowRequest request, ServerCallContext context)
        {
            var deleteResult = await _cabinetRepository.DeleteRowAsync(request.CabinetNumber, request.RowNumber, context.CancellationToken);
            if (deleteResult.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Row not found"));
            }

            return new Empty();
        }

        public override async Task<Lane> CreateLane(CreateLaneRequest request, ServerCallContext context)
        {
            var nextCabinetNumber = await _cabinetRepository.GetNextLaneNumberInRowAsync(request.CabinetNumber, request.RowNumber, context.CancellationToken);
            request.Lane.Number = nextCabinetNumber;

            var convertedRow = _laneConverter.Convert(request.Lane);

            var result = await _cabinetRepository.CreateLaneAsync(request.CabinetNumber,request.RowNumber, convertedRow, context.CancellationToken);
            if (result.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.Aborted, "Lane was not created"));
            }

            return request.Lane;
        }

        public override async Task<Lane> GetLane(GetLaneRequest request, ServerCallContext context)
        {
            var cabinet = await _cabinetRepository.GetLaneAsync(request.CabinetNumber, request.RowNumber, request.LaneNumber, context.CancellationToken);
            if (cabinet == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Lane not found"));
            }

            var cabinetProto = _laneConverter.Convert(cabinet);
            return cabinetProto;
        }

        public override async Task<Lane> UpdateLane(UpdateLaneRequest request, ServerCallContext context)
        {
            var cabinet = await _cabinetRepository.UpdateLaneAsync(request.CabinetNumber, request.RowNumber, request.Lane.Number, request.Lane.JanCode, request.Lane.Quantity, context.CancellationToken);
            if (cabinet == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Lane not found"));
            }

            return request.Lane;
        }

        public override async Task<Empty> DeleteLane(DeleteLaneRequest request, ServerCallContext context)
        {
            var deleteResult = await _cabinetRepository.DeleteLaneAsync(request.CabinetNumber, request.RowNumber, request.LaneNumber, context.CancellationToken);
            if (deleteResult.ModifiedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Lane not found"));
            }

            return new Empty();
        }


        /// <summary>
        /// Swaps the positions of two lanes regardless of row or cabinet position.
        /// </summary>
        /// <param name="request">Contains the first and second JanCodes to be swapped.</param>
        /// <param name="context">ServerCallContext object for handling the gRPC request context.</param>
        /// <returns>
        /// Returns true if the swap is successful, indicating that the lanes were found and swapped.
        /// If any of the lanes are not found, it returns false. If there are any errors during the swap process,
        /// it returns false as well. The operation is performed within a transaction to ensure atomicity.
        /// </returns>
        public override async Task<SwapProductByLanesResponse> SwapProductByLanes(SwapProductByLanesRequest request, ServerCallContext context)
        {
            if (request.SourceLaneNumber == request.TargetLaneNumber && request.SourceCabinetNumber == request.TargetCabinetNumber && request.SourceRowNumber == request.TargetRowNumber)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Lanes are the same"));
            }

            var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var targetLane = await _cabinetRepository.GetLaneAsync(request.TargetCabinetNumber, request.TargetRowNumber, request.TargetLaneNumber);
                var sourceLane = await _cabinetRepository.GetLaneAsync(request.SourceCabinetNumber, request.SourceRowNumber, request.SourceLaneNumber);
                if (targetLane == null || sourceLane == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "One or both lanes are not found"));
                }

                // Swap JanCode and Quantity data between lanes.
                await _cabinetRepository.UpdateLaneAsync(request.TargetCabinetNumber, request.TargetRowNumber, request.TargetLaneNumber, sourceLane.JanCode, sourceLane.Quantity, context.CancellationToken);
                await _cabinetRepository.UpdateLaneAsync(request.SourceCabinetNumber, request.SourceRowNumber, request.SourceLaneNumber, targetLane.JanCode, targetLane.Quantity, context.CancellationToken);

                await session.CommitTransactionAsync();
                return new SwapProductByLanesResponse
                {
                    Result = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await session.AbortTransactionAsync();
                throw new RpcException(new Status(StatusCode.Internal, "Unable to swap lanes"));
            }
        }
    }
}
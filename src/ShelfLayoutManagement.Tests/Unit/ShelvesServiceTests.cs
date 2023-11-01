using MongoDB.Driver;

using Moq;

using NUnit.Framework;

using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Common.Shelf;
using ShelfLayoutManagement.Data.Repositories;
using ShelfLayoutManagement.Server.Services;

namespace ShelfLayoutManagement.Tests.Unit
{
    [TestFixture]
    public class ShelvesServiceTests
    {
        private ShelvesService _shelvesService;
        private Mock<ICabinetRepository> _mockCabinetRepository;
        private Mock<IConverter<Data.Entities.Cabinet, Common.Shelf.Cabinet>> _mockCabinetConverter;
        private Mock<IConverter<Data.Entities.Row, Common.Shelf.Row>> _mockRowConverter;
        private Mock<IConverter<Data.Entities.Lane, Common.Shelf.Lane>> _mockLaneConverter;

        [SetUp]
        public void Initialize()
        {
            _mockCabinetRepository = new Mock<ICabinetRepository>();
            _mockCabinetConverter = new Mock<IConverter<Data.Entities.Cabinet, Common.Shelf.Cabinet>>();
            _mockRowConverter = new Mock<IConverter<Data.Entities.Row, Common.Shelf.Row>>();
            _mockLaneConverter = new Mock<IConverter<Data.Entities.Lane, Common.Shelf.Lane>>();

            _shelvesService = new ShelvesService(
                cabinetRepository: _mockCabinetRepository.Object,
                cabinetConverter: _mockCabinetConverter.Object,
                rowConverter: _mockRowConverter.Object,
                laneConverter: _mockLaneConverter.Object,
                mongoClient: null,
                logger: null
            );
        }

        [Test]
        public async Task CreateCabinet_Valid_ReturnsCabinet()
        {
            var request = new CreateCabinetRequest
            {
                Cabinet = new Cabinet
                {
                    Position = new Cabinet.Types.Position()
                    {
                        X = 5,
                        Y = 5,
                        Z = 5
                    },
                    Size = new Cabinet.Types.Size()
                    {
                        Depth = 10,
                        Height = 10,
                        Width = 10
                    },
                    Rows =
                    {
                        new Row
                        {
                            Number = 1,
                            PositionZ = 10,
                            Size = new Row.Types.Size()
                            {
                                Height = 10
                            },
                            Lanes =
                            {
                                new Lane()
                                {
                                    JanCode = "xxxx1",
                                    Number = 1,
                                    PositionX = 10,
                                    Quantity = 45
                                },
                                new Lane()
                                {
                                    JanCode = "xxxx2",
                                    Number = 2,
                                    PositionX = 10,
                                    Quantity = 45
                                }
                            }
                        }
                    }
                }
            };
            var expectedCabinet = new Data.Entities.Cabinet();
            _mockCabinetConverter.Setup(converter => converter.Convert(It.IsAny<Cabinet>())).Returns(expectedCabinet);
            var result = await _shelvesService.CreateCabinet(request, null);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetCabinet_ExistingCabinetNumber_ReturnsCabinet()
        {
            var request = new GetCabinetRequest
            {
                CabinetNumber = 1 
            };
            var expectedCabinet = new Data.Entities.Cabinet();
            _mockCabinetRepository.Setup(repo => repo.GetCabinetAsync(request.CabinetNumber, CancellationToken.None))
                                 .ReturnsAsync(expectedCabinet);
            _mockCabinetConverter.Setup(converter => converter.Convert(It.IsAny<Data.Entities.Cabinet>()))
                                 .Returns(new Common.Shelf.Cabinet());

            var result = await _shelvesService.GetCabinet(request, null);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateRow_Valid_ReturnsCreatedRow()
        {
            var request = new CreateRowRequest
            {
                CabinetNumber = 1,
                Row = new Common.Shelf.Row {
                    Number = 1,
                    Size = new Row.Types.Size()
                    {
                        Height = 10
                    },
                    PositionZ = 10
                }
            };
            var expectedRow = new Data.Entities.Row();
            _mockRowConverter.Setup(converter => converter.Convert(It.IsAny<Common.Shelf.Row>())).Returns(expectedRow);
            _mockCabinetRepository.Setup(repo => repo.CreateRowAsync(request.CabinetNumber, expectedRow, CancellationToken.None))
                                 .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var result = await _shelvesService.CreateRow(request, null);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateLane_Valid_ReturnsCreatedLane()
        {
            var request = new CreateLaneRequest
            {
                CabinetNumber = 1, 
                RowNumber = 1,
                Lane = new Common.Shelf.Lane
                {
                    Number = 1,
                    JanCode = "xxx1",
                    PositionX = 10,
                    Quantity = 1
                }
            };
            var expectedLane = new Data.Entities.Lane();
            _mockLaneConverter.Setup(converter => converter.Convert(It.IsAny<Common.Shelf.Lane>())).Returns(expectedLane);
            _mockCabinetRepository.Setup(repo => repo.CreateLaneAsync(request.CabinetNumber, request.RowNumber, expectedLane, CancellationToken.None))
                                 .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var result = await _shelvesService.CreateLane(request, null);

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task SwapProductByLanes_Valid_ReturnsTrue()
        {
            var request = new SwapProductByLanesRequest
            {
                SourceCabinetNumber = 1,
                SourceRowNumber = 1,
                SourceLaneNumber = 1,
                TargetCabinetNumber = 2,
                TargetRowNumber = 2,
                TargetLaneNumber = 2
            };

            _mockCabinetRepository.Setup(repo => repo.GetLaneAsync(request.SourceCabinetNumber, request.SourceRowNumber, request.SourceLaneNumber,default))
                                 .ReturnsAsync(new Data.Entities.Lane());
            _mockCabinetRepository.Setup(repo => repo.GetLaneAsync(request.TargetCabinetNumber, request.TargetRowNumber, request.TargetLaneNumber, default))
                                 .ReturnsAsync(new Data.Entities.Lane());

            _mockCabinetRepository.Setup(repo => repo.UpdateLaneAsync(request.TargetCabinetNumber, request.TargetRowNumber, request.TargetLaneNumber,
                It.IsAny<string>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            _mockCabinetRepository.Setup(repo => repo.UpdateLaneAsync(request.SourceCabinetNumber, request.SourceRowNumber, request.SourceLaneNumber,
                It.IsAny<string>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var result = await _shelvesService.SwapProductByLanes(request, null);

            Assert.IsTrue(result.Result);
        }

    }
}
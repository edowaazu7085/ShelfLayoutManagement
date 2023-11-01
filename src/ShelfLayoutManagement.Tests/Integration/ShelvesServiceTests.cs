using Grpc.Core;
using Grpc.Net.Client;

using NUnit.Framework;

using ShelfLayoutManagement.Common.Shelf;
using ShelfLayoutManagement.Server.Services;

namespace ShelfLayoutManagement.Tests.Integration
{
    [TestFixture]
    public class ShelvesServiceTests : IntegrationTestBase
    {


        [Test]
        public async Task ReadCabinet_ShouldReturnCorrectCabinet()
        {
            using var channel = CreateChannel();
            var call = new Shelves.ShelvesClient(channel).StreamCabinets(new StreamCabinetsRequest());
            var count = 0;
            while (await call.ResponseStream.MoveNext())
            {
                Console.WriteLine("Cabinet: Number:" + call.ResponseStream.Current.Number + " Position:X" + call.ResponseStream.Current.Position.X + " Y" + call.ResponseStream.Current.Position.Y + " Z" + call.ResponseStream.Current.Position.Z);
                count++;
            }

            Assert.Greater(count, 0);
        }

        [Test]
        public void CreateCabinet_ShouldCreateNewCabinet()
        {
            using var channel = CreateChannel();

            var newCabinet = new Cabinet
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
            };

            var call = new Shelves.ShelvesClient(channel).CreateCabinet(new CreateCabinetRequest{Cabinet = newCabinet });
            
            Assert.Greater(call.Number, 0);
        }

        [Test]
        public async Task SwapProductByLanesAsync_Valid_ReturnsCabinet()
        {
            var request = new SwapProductByLanesRequest()
            {
                SourceCabinetNumber = 1,
                SourceLaneNumber = 1,
                SourceRowNumber = 1,
                TargetCabinetNumber = 2,
                TargetLaneNumber = 2,
                TargetRowNumber = 2
            };

            using var channel = CreateChannel();
            var result = await new Shelves.ShelvesClient(channel).SwapProductByLanesAsync(request, new CallOptions());
            
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task SwapProductByLanesAsync_SameTarget_ReturnsError()
        {
            var request = new SwapProductByLanesRequest()
            {
                SourceCabinetNumber = 1,
                SourceLaneNumber = 1,
                SourceRowNumber = 1,
                TargetCabinetNumber = 1,
                TargetLaneNumber = 1,
                TargetRowNumber = 1
            };

            using var channel = CreateChannel();
            var client = new Shelves.ShelvesClient(channel);

            Assert.CatchAsync<RpcException>(
                async () => await client.SwapProductByLanesAsync(request, new CallOptions()));
        }

        [Test]
        public async Task ListCabinetsAsync_WithPagination_ReturnsList()
        {
            var request = new ListCabinetsRequest()
            {
                PageSize = 10,
                Page = 1,
                OrderBy = CabinetField.Number
            };

            using var channel = CreateChannel();
            var result = await new Shelves.ShelvesClient(channel).ListCabinetsAsync(request, new CallOptions());

            Assert.AreEqual(result.Cabinets.Count, 10);
        }

        [Test]
        public async Task ListCabinetsAsync_NoPagination_ReturnsWholeList()
        {
            var request = new ListCabinetsRequest()
            {
                OrderBy = CabinetField.Number
            };

            using var channel = CreateChannel();
            var result = await new Shelves.ShelvesClient(channel).ListCabinetsAsync(request, new CallOptions());

            Assert.GreaterOrEqual(result.Cabinets.Count, 10);
        }
    }
}
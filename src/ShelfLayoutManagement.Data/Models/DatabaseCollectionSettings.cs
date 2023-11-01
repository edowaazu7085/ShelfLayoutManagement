namespace ShelfLayoutManagement.Data.Models
{
    public interface IDatabaseCollectionSettings
    {
        string CollectionName { get; set; }
        string DatabaseName { get; set; }
    }

    public interface ICabinetCollectionSettings : IDatabaseCollectionSettings
    {
    }

    public interface IStockKeepingUnitCollectionSettings : IDatabaseCollectionSettings
    {
    }

    public class CabinetCollectionSettings : ICabinetCollectionSettings
    {
        public string CollectionName { get; set; }

        public string DatabaseName { get; set; }
    }

    public class StockKeepingUnitCollectionSettings : IStockKeepingUnitCollectionSettings
    {
        public string CollectionName { get; set; }

        public string DatabaseName { get; set; }
    }
}

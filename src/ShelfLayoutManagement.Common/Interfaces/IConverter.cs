namespace ShelfLayoutManagement.Common.Interfaces
{
    public interface IConverter<TSource, TDestination>
    {
        TDestination Convert(TSource source);
        TSource Convert(TDestination destination);
    }
}

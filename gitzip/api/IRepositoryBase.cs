namespace gitzip.api
{
    public interface IRepositoryBase
    {
        string FetchRepository(string url, long? revision);
    }
}
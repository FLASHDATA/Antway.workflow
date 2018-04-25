namespace WorkFlow.Interfaces
{
    public interface IDAL
    {
        T Fetch<T>(object pk);
        T Update<T>(T objectView);
        T Insert<T>(T objectView);
        bool Delete<T>(T objectView);
    }
}

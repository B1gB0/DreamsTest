namespace Game.Services.Storage
{
    public interface IStorageService
    {
        void Save<T>(string key, T data) where T : class;
        T Load<T>(string key) where T : class;
        bool Has(string key);
        void Delete(string key);
    }
}


namespace lhFramework.Infrastructure.Core
{
    public delegate void EventHandler();
    public delegate void DataHandler<T>(T t);
    public delegate void LoadHandler(int id, DataHandler<UnityEngine.Object> handler);
    public delegate void LoadEntireHandler(int id, DataHandler<UnityEngine.Object[]> handler);
    public delegate void DestroyHandler(int id);
}
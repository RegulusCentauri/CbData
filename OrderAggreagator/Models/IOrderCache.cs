using System.Collections.Concurrent;

namespace OrderAggregator
{
    public interface IOrderCache
    {
        void AddOrder(Order order);
        ConcurrentDictionary<string, int> GetAggregatedOrders();
        void ClearOrders();
    }
}

using System.Collections.Concurrent;

namespace OrderAggregator
{
    public class OrderCache : IOrderCache
    {
        private readonly ConcurrentDictionary<string, int> _orders = [];

        public void AddOrder(Order order) {
            _orders.AddOrUpdate(order.ProductId, order.Quantity, (_, existingQty) => existingQty + order.Quantity);
        }

        public ConcurrentDictionary<string, int> GetAggregatedOrders() {
            return new ConcurrentDictionary<string, int>(_orders);
        }

        public void ClearOrders() {
            _orders.Clear();
        }
    }
}

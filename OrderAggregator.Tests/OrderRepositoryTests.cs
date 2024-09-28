namespace OrderAggregator.Tests
{
    public class OrderRepositoryTests
    {
        [Fact]
        public void AddOrder_ShouldAddOrder() {
            // Arrange
            var orderQuantity = 10;
            var productId = "666";
            var repository = new OrderCache();
            var order = new Order { ProductId = productId, Quantity = orderQuantity };

            // Act
            repository.AddOrder(order);

            // Assert
            var aggregatedOrders = repository.GetAggregatedOrders();
            Assert.Equal(orderQuantity, aggregatedOrders[productId]);
        }

        [Fact]
        public void AddOrder_ShouldAggregateQuantity() {
            var firstOrderQuantity = 10;
            var secondOrderQuantity = 5;
            var productId = "666";
            var repository = new OrderCache();
            var order1 = new Order { ProductId = productId, Quantity = firstOrderQuantity };
            var order2 = new Order { ProductId = productId, Quantity = secondOrderQuantity };

            repository.AddOrder(order1);
            repository.AddOrder(order2);

            var aggregatedOrders = repository.GetAggregatedOrders();
            Assert.Single(aggregatedOrders);
            Assert.Equal(firstOrderQuantity + secondOrderQuantity, aggregatedOrders[productId]);
        }

        [Fact]
        public void ClearOrders_ShouldEmptyRepository() {
            var repository = new OrderCache();
            var order = new Order { ProductId = "666", Quantity = 10 };
            repository.AddOrder(order);

            repository.ClearOrders();

            var aggregatedOrders = repository.GetAggregatedOrders();
            Assert.Empty(aggregatedOrders);
        }
    }
}

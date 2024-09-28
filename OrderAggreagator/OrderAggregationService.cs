using System.Text.Json;

namespace OrderAggregator
{
    public class OrderAggregationService : BackgroundService
    {
        readonly IOrderCache _orderRepository;
        readonly ILogger<OrderAggregationService> _logger;
        readonly TimeSpan _aggregationInterval = TimeSpan.FromSeconds(20); //Upgrade suggestion: Move to appsettings, so the param is configurable

        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };

        public OrderAggregationService(IOrderCache orderRepository, ILogger<OrderAggregationService> logger) {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Order Aggregation Service started.");

            while(!cancellationToken.IsCancellationRequested) {
                try {
                    await Task.Delay(_aggregationInterval, cancellationToken);

                    var aggregatedOrders = _orderRepository.GetAggregatedOrders();
                    if(aggregatedOrders.Any()) {
                        var aggregatedOrderList = aggregatedOrders.Select(ao => new Order {
                            ProductId = ao.Key,
                            Quantity = ao.Value
                        }).ToList();

                        var json = JsonSerializer.Serialize(aggregatedOrderList, jsonSerializerOptions);

                        // Simulate sending to internal system
                        Console.WriteLine("Sending aggregated orders to internal system:");
                        Console.WriteLine(json);

                        _orderRepository.ClearOrders();
                        _logger.LogInformation("Aggregated orders sent and cleared.");
                    }
                    else {
                        _logger.LogInformation("No orders to aggregate at this time.");
                    }
                }
                catch(Exception e) {
                    _logger.LogInformation($"Exception thrown: {e}");
                }
            }

            _logger.LogInformation("Order Aggregation Service stopped.");
        }
    }
}

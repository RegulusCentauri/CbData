using Microsoft.AspNetCore.Mvc;

namespace OrderAggregator
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderCache _orderRepository;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderCache orderRepository, ILogger<OrdersController> logger) {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        //Upgrade suggestion: Adding an extra security layer to make sure the service can't be called by just anybody - ApiKey header or something along those lines
        [HttpPost("PostOrders")]
        public IActionResult PostOrders([FromBody] List<Order> orders) {
            return HandleIncomingOrders(orders);
        }

        public IActionResult HandleIncomingOrders(List<Order> orders) {
            try {
                if(orders == null || !orders.Any()) {
                    _logger.LogWarning("Empty order list received");
                    return BadRequest("Order list cannot be empty.");
                }

                foreach(var order in orders) {
                    if(string.IsNullOrEmpty(order.ProductId)) {
                        _logger.LogWarning($"ProductId was null or empty: {order}");
                        return BadRequest("Invalid ProductId.");
                    }
                    if(order.Quantity <= 0) {
                        _logger.LogWarning($"Quantity <= 0: {order}");
                        return BadRequest("Product quantity has to be bigger than zero.");
                    }

                    _orderRepository.AddOrder(order);
                }

                _logger.LogInformation($"Received and stored {orders.Count} orders.");
                return Accepted();
            }
            catch(Exception e) {
                _logger.LogInformation($"Exception thrown: {e}");
                return Problem(detail: $"{e}", title: "Unexpected error");
            }
        }
    }
}

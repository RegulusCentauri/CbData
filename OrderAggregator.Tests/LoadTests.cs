using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Net.Http.Json;

namespace OrderAggregator.Tests
{
    public class LoadTests
    {
        private readonly HttpClient _httpClient;

        public LoadTests() {
            _httpClient = new HttpClient {
                BaseAddress = new Uri("https://localhost:7042") //This should be replaced by a different URL as per Upgrade suggestion below
            };
        }

        [Fact]
        //Upgrade suggestion: Best would be to use HttpClient to call the PostOrders() directly, but that's getting blocked through localhost (some remote deployment would be necessary to run it)
        public async Task LoadTest_RequestNumber_Local() {
            int totalRequests = 100; // Number of simulated requests
            var tasks = new List<Task>();

            var orderCache = new OrderCache();
            var logger = NullLogger<OrdersController>.Instance;

            var controller = new OrdersController(orderCache, logger);

            for(int i = 0 ; i < totalRequests ; i++) {
                var index = i.ToString(); // Capture the current value of i, otherwise the Task.Run uses the same "i" value for each run

                tasks.Add(Task.Run(() => {
                    // Prepare orders
                    var orders = new List<Order>
                    {
                        new Order
                        {
                            ProductId = index,
                            Quantity = 1
                        }
                    };

                    var result = controller.HandleIncomingOrders(orders);

                    // Assert that the result is Accepted
                    Assert.IsType<AcceptedResult>(result);
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            var aggregatedOrders = orderCache.GetAggregatedOrders();

            // Verify that total order nr is correct
            int totalOrders = aggregatedOrders.Count;
            Assert.Equal(totalRequests, totalOrders);
        }


        [Fact]
        public async Task LoadTest_RequestNumber_HttpClient() {
            int totalRequests = 100;
            var tasks = new List<Task<HttpResponseMessage>>();

            for(int i = 0 ; i < totalRequests ; i++) {
                tasks.Add(SendOrderRequest(i));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Verify the responses
            foreach(var task in tasks) {
                var response = await task;
                Assert.True(response.IsSuccessStatusCode, $"Request failed with status code {response.StatusCode}");
            }
        }

        async Task<HttpResponseMessage> SendOrderRequest(int index) {
            // Simulate a small delay to spread the requests over a second
            await Task.Delay(index * 10); // Adjust the multiplier to fine-tune the request rate

            var orders = new List<Order>
            {
                new Order { ProductId = index.ToString(), Quantity = 1 }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/orders/PostOrders", orders);
            return response;
        }

        // In theory, only one Assert per test should be present, as it should test just ONE thing, but with this setup,
        // not using the previous asserts could lead to a passed test, but with invalid steps
        [Fact]
        public async Task LoadTest_ProductIdCount_Local() {
            int totalRequests = 700; // Number of simulated ProductIds
            var tasks = new List<Task>();

            var orderCache = new OrderCache();
            var logger = NullLogger<OrdersController>.Instance;

            var controller = new OrdersController(orderCache, logger);

            //Timer setup
            var timer = new Stopwatch();
            timer.Start();

            for(int i = 0 ; i < totalRequests ; i++) {
                var index = i.ToString(); // Capture the current value of i, otherwise the Task.Run uses the same "i" value for each run

                tasks.Add(Task.Run(() => {
                    // Prepare orders
                    var orders = new List<Order>
                    {
                        new Order
                        {
                            ProductId = index,
                            Quantity = 1
                        }
                    };

                    var result = controller.HandleIncomingOrders(orders);

                    // Assert that the result is Accepted
                    Assert.IsType<AcceptedResult>(result);
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            //Timer stop
            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed; //Time it took to process all the orders using the current solution

            var aggregatedOrders = orderCache.GetAggregatedOrders();

            // Verify that total order nr is correct
            int totalOrders = aggregatedOrders.Count;
            Assert.Equal(totalRequests, totalOrders);
            Assert.True(timeTaken < TimeSpan.FromSeconds(1), $"Time taken to take {totalRequests} was too long ({timeTaken})");
        }
    }
}

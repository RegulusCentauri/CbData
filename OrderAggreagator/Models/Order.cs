namespace OrderAggregator
{
    public class Order
    {
        public string? ProductId { get; set; } //VS doesn't like string without the nullable mark
        public int Quantity { get; set; }
    }
}

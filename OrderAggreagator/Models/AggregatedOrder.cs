namespace OrderAggregator
{
    public class AggregatedOrder
    {
        public string? ProductId { get; set; } //VS doesn't like string without the nullable mark
        public int TotalQuantity { get; set; }
    }
}

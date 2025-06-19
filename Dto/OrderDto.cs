namespace WashUpAPIFix.Dto
{
    public class OrderDto
    {
        public int LaundryOrderId { get; set; }
        public string Status { get; set; }
        public string PickupAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new();
        public PaymentDto? Payment { get; set; }
        public RatingDto? Rating { get; set; }
    }

    public class OrderDetailDto
    {
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PaymentDto
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime PaidAt { get; set; }
        public string PaymentProofUrl { get; set; }
    }

    public class RatingDto
    {
        public int Score { get; set; }
        public string Comment { get; set; }
        public DateTime RatedAt { get; set; }
    }
}

namespace StockManagementWebApplication.Models.DTO
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }
}

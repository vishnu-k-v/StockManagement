namespace StockManagementWebApplication.Models.DTO
{
    public class ModifyCartDTO
    {
        public string Name { get; set; }
        public int OrderItemId { get; set; }
        public int ItemId { get; set; }
        public double Rate { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
    }
}

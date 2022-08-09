using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace StockManagementWebApplication.Models.Entities
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double Rate { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public virtual Item Item { get; set; }
        public virtual Order Order { get; set; }

    }
}

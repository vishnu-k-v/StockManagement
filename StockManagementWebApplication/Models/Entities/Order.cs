using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementWebApplication.Models.Entities
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string OrderdBy { get; set; }
        public DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public virtual ApplicationUser OrderUser { get; set; }
        public virtual List<OrderItem> OrderItems { get; set; }



    }
}

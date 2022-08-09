using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementWebApplication.Models.Entities
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Rate { get; set; }
        public double Quantity { get; set; }
        public virtual ICollection<OrderItem> Orders { get; set; }
    }
}

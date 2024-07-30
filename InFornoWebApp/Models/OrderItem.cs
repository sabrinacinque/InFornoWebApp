using System.ComponentModel.DataAnnotations;

namespace InFornoWebApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal TotalPrice => Quantity * Product.Price;
    }
}

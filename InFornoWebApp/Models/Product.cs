using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InFornoWebApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string PhotoUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int DeliveryTime { get; set; } // Time in minutes

        public List<ProductIngredient> ProductIngredients { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InFornoWebApp.Models
{
    public class Product
    {
        public int  Id { get; set; }

      
        public string  Name { get; set; }

        public string ? PhotoUrl { get; set; }

        
        public decimal  Price { get; set; }

       
        public int ? DeliveryTime { get; set; } // tempo in minuti

        public List<ProductIngredient> ?  ProductIngredients { get; set; } = new List<ProductIngredient>();
    }
}

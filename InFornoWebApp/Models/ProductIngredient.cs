namespace InFornoWebApp.Models
{
    public class ProductIngredient
    {
        public int   ProductId { get; set; }
        public Product  Product { get; set; }

        public int  IngredientId { get; set; }
        public Ingredient  Ingredient { get; set; }
    }
}

 //questa classe ci serve per gestire la relazione "molti a molti" , perchè ogni pizza può avere più
 //ingredienti, e ogni ingrediente può essere associato a più pizze 

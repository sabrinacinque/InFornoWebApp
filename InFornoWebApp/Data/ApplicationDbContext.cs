using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InFornoWebApp.Models;

namespace InFornoWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductIngredient>()
                .HasKey(pi => new { pi.ProductId, pi.IngredientId });

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Ingredient)
                .WithMany(i => i.ProductIngredients)
                .HasForeignKey(pi => pi.IngredientId);

            // Aggiungo qualche inngrediente in modo che posso testare le crud dei prodotti
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Mozzarella" },
                new Ingredient { Id = 2, Name = "Pomodoro" },
                new Ingredient { Id = 3, Name = "Basilico" },
                new Ingredient { Id = 4, Name = "Olio d'oliva" },
                new Ingredient { Id = 5, Name = "Sale" },
                new Ingredient { Id = 6, Name = "Pepe" },
                new Ingredient { Id = 7, Name = "Aglio" },
                new Ingredient { Id = 8, Name = "Origano" },
                new Ingredient { Id = 9, Name = "Prosciutto" },
                new Ingredient { Id = 10, Name = "Funghi" },
                new Ingredient { Id = 11, Name = "Cipolla" },
                new Ingredient { Id = 12, Name = "Peperoni" },
                new Ingredient { Id = 13, Name = "Carciofi" },
                new Ingredient { Id = 14, Name = "Acciughe" },
                new Ingredient { Id = 15, Name = "Salsiccia" }
            );
        }
    }
}

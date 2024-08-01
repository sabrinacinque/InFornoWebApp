using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InFornoWebApp.Data;
using InFornoWebApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace InFornoWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageProducts()
        {
            return View(_context.Products.Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient).ToList());
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product? product, IFormFile? photo, int[]? selectedIngredients)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }
                    product.PhotoUrl = "/images/" + photo.FileName;
                }

                _context.Products.Add(product);
                _context.SaveChanges();

                if (selectedIngredients != null)
                {
                    foreach (var ingredientId in selectedIngredients)
                    {
                        _context.ProductIngredients.Add(new ProductIngredient
                        {
                            ProductId = product.Id,
                            IngredientId = ingredientId
                        });
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction(nameof(ManageProducts));
            }
            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View(product);
        }

        public IActionResult EditProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductIngredients)
                .SingleOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(int? id, Product? product, IFormFile? photo, int[]? selectedIngredients)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products
                    .Include(p => p.ProductIngredients)
                    .SingleOrDefault(p => p.Id == id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (photo != null && photo.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }
                    existingProduct.PhotoUrl = "/images/" + photo.FileName;
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.DeliveryTime = product.DeliveryTime;

                _context.ProductIngredients.RemoveRange(existingProduct.ProductIngredients);

                if (selectedIngredients != null)
                {
                    foreach (var ingredientId in selectedIngredients)
                    {
                        _context.ProductIngredients.Add(new ProductIngredient
                        {
                            ProductId = product.Id,
                            IngredientId = ingredientId
                        });
                    }
                }

                _context.SaveChanges();
                return RedirectToAction(nameof(ManageProducts));
            }
            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductIngredients)
                .SingleOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageProducts));
        }

        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.IsCompleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageOrders));
        }
    }
}

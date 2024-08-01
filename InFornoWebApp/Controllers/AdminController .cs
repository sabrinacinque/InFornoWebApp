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
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
        public async Task<IActionResult> CreateProduct(Product product, IFormFile photo, int[] selectedIngredients)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    var filePath = Path.Combine(_env.WebRootPath, "img", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    product.PhotoUrl = "/img/" + photo.FileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

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
                    await _context.SaveChangesAsync();
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
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile photo, int[] selectedIngredients)
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
                    var filePath = Path.Combine(_env.WebRootPath, "img", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    existingProduct.PhotoUrl = "/img/" + photo.FileName;
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
                            ProductId = existingProduct.Id,
                            IngredientId = ingredientId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageProducts));
            }
            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductIngredients)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Prodotto non trovato." });
            }

            if (!string.IsNullOrEmpty(product.PhotoUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, product.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
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

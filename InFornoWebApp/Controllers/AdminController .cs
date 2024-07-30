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
        public async Task<IActionResult> CreateProduct(Product product, IFormFile photo, int[] selectedIngredients)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    product.PhotoUrl = "/images/" + photo.FileName;
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
        public async Task<IActionResult> EditProduct(Product product, IFormFile photo, int[] selectedIngredients)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products
                    .Include(p => p.ProductIngredients)
                    .SingleOrDefault(p => p.Id == product.Id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (photo != null && photo.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    product.PhotoUrl = "/images/" + photo.FileName;
                }
                else
                {
                    product.PhotoUrl = existingProduct.PhotoUrl;
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.DeliveryTime = product.DeliveryTime;
                existingProduct.PhotoUrl = product.PhotoUrl;

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

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageProducts));
            }
            ViewBag.Ingredients = _context.Ingredients.ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = _context.Products
                .Include(p => p.ProductIngredients)
                .SingleOrDefault(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}

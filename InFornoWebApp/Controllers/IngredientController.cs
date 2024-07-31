using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InFornoWebApp.Data;
using InFornoWebApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace InFornoWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class IngredientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Ingredients.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        public IActionResult Edit(int id)
        {
            var ingredient = _context.Ingredients.SingleOrDefault(i => i.Id == id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                var existingIngredient = _context.Ingredients.SingleOrDefault(i => i.Id == ingredient.Id);
                if (existingIngredient == null)
                {
                    return NotFound();
                }

                existingIngredient.Name = ingredient.Name;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = _context.Ingredients.SingleOrDefault(i => i.Id == id);
            if (ingredient == null)
            {
                return Json(new { success = false, message = "Ingrediente non trovato." });
            }

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}

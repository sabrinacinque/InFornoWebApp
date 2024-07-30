using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InFornoWebApp.Data;
using InFornoWebApp.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InFornoWebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult OrderSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = _context.Orders
                .Where(o => o.UserId == userId && !o.IsCompleted)
                .FirstOrDefault();

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = _context.Orders
                .Where(o => o.UserId == userId && !o.IsCompleted)
                .FirstOrDefault();

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            order.IsCompleted = true;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderHistory");
        }

        public IActionResult OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _context.Orders
                .Where(o => o.UserId == userId && o.IsCompleted)
                .ToList();

            return View(orders);
        }
    }
}

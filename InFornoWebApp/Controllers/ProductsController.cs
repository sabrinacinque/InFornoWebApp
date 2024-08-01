using Microsoft.AspNetCore.Mvc;
using InFornoWebApp.Data;
using InFornoWebApp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        var products = _context.Products
            .Include(p => p.ProductIngredients)
            .ThenInclude(pi => pi.Ingredient)
            .ToList();

        ViewBag.CartCount = GetCartCount();
        return View(products);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int id)
    {
        var product = _context.Products.SingleOrDefault(p => p.Id == id);
        if (product == null)
        {
            return Json(new { success = false, message = "Prodotto non trovato." });
        }

        var cart = GetCart();
        if (cart.ContainsKey(id))
        {
            cart[id]++;
        }
        else
        {
            cart[id] = 1;
        }

        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

        return Json(new { success = true, cartCount = cart.Values.Sum() });
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int id, int quantity)
    {
        var product = _context.Products.SingleOrDefault(p => p.Id == id);
        if (product == null)
        {
            return Json(new { success = false, message = "Prodotto non trovato." });
        }

        var cart = GetCart();
        if (cart.ContainsKey(id))
        {
            cart[id] = quantity;
        }
        else
        {
            return Json(new { success = false, message = "Prodotto non trovato nel carrello." });
        }

        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

        var productTotal = (decimal)(product.Price * quantity);
        var cartTotal = cart.Sum(item => (decimal)(_context.Products.SingleOrDefault(p => p.Id == item.Key).Price * item.Value));

        return Json(new { success = true, productTotal, cartTotal });
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteFromCart(int id)
    {
        var cart = GetCart();
        if (cart.ContainsKey(id))
        {
            cart.Remove(id);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Prodotto non trovato nel carrello." });
    }

    [Authorize(Roles = "User")]
    public IActionResult Cart()
    {
        var cart = GetCart();
        var products = _context.Products
            .Where(p => cart.Keys.Contains(p.Id))
            .Include(p => p.ProductIngredients)
            .ThenInclude(pi => pi.Ingredient)
            .ToList();

        ViewBag.Quantities = cart;

        return View(products);
    }

    [Authorize(Roles = "User")]
    public IActionResult Checkout()
    {
        var cart = GetCart();
        var products = _context.Products
            .Where(p => cart.Keys.Contains(p.Id))
            .Include(p => p.ProductIngredients)
            .ThenInclude(pi => pi.Ingredient)
            .ToList();

        ViewBag.Quantities = cart;

        return View(products);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string shippingAddress, string notes, string deliveryTime)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = GetCart();
        var products = _context.Products
            .Where(p => cart.Keys.Contains(p.Id))
            .Include(p => p.ProductIngredients)
            .ThenInclude(pi => pi.Ingredient)
            .ToList();

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = shippingAddress,
            Notes = notes,
            IsCompleted = false,
            OrderDate = DateTime.Now,
            OrderItems = products.Select(p => new OrderItem
            {
                ProductId = p.Id,
                Quantity = cart[p.Id]
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Cart");

        return RedirectToAction("OrderConfirmation");
    }

    [Authorize(Roles = "User")]
    public IActionResult OrderConfirmation()
    {
        return View();
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> OrderHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .ToList();

        return View(orders);
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .SingleOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    private Dictionary<int, int> GetCart()
    {
        var cart = HttpContext.Session.GetString("Cart");
        if (cart == null)
        {
            return new Dictionary<int, int>();
        }
        return JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
    }

    private int GetCartCount()
    {
        var cart = GetCart();
        return cart.Values.Sum();
    }
}

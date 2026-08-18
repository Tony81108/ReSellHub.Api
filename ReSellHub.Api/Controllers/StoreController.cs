using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;

namespace ReSellHub.Api.Controllers;

public class StoreController : Controller
{
    private readonly AppDbContext _context;

    public StoreController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? keyword, string filter = "all", int? categoryId = null)
    {
        var query = _context.Products.AsNoTracking().Include(product => product.Category).Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(product => product.Name.Contains(keyword));
        }

        query = filter switch
        {
            "in-stock" => query.Where(product => product.StockQuantity > 0),
            "under-500" => query.Where(product => product.SellingPrice < 500),
            "500-1500" => query.Where(product => product.SellingPrice >= 500 && product.SellingPrice <= 1500),
            "over-1500" => query.Where(product => product.SellingPrice > 1500),
            _ => query
        };

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        ViewBag.Keyword = keyword;
        ViewBag.Filter = filter;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await _context.Categories.AsNoTracking().Where(category => category.IsActive).OrderBy(category => category.SortOrder).ToListAsync();
        ViewBag.TotalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(product => product.Id)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id);

        return product is null ? NotFound() : View(product);
    }
}

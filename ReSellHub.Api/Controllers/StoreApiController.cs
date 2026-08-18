using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;

namespace ReSellHub.Api.Controllers;

[ApiController]
[Route("api/store")]
public class StoreApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoreApiController(AppDbContext context) => _context = context;

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _context.Categories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Name, x.Slug, x.Icon })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products(string? keyword, string filter = "all", int? categoryId = null)
    {
        var query = _context.Products.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(x => x.Name.Contains(keyword));
        }

        query = filter switch
        {
            "in-stock" => query.Where(x => x.StockQuantity > 0),
            "under-500" => query.Where(x => x.SellingPrice < 500),
            "500-1500" => query.Where(x => x.SellingPrice >= 500 && x.SellingPrice <= 1500),
            "over-1500" => query.Where(x => x.SellingPrice > 1500),
            _ => query
        };

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        var products = await query.OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id, x.Name, x.SellingPrice, x.StockQuantity, x.Condition,
                x.CoverImageUrl, x.Description,
                Category = x.Category == null ? null : new { x.Category.Id, x.Category.Name, x.Category.Icon }
            }).ToListAsync();

        return Ok(new { total = products.Count, items = products });
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> Product(int id)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new
            {
                x.Id, x.Name, x.SellingPrice, x.StockQuantity, x.Condition,
                x.CoverImageUrl, x.Description,
                Category = x.Category == null ? null : new { x.Category.Id, x.Category.Name, x.Category.Icon }
            }).FirstOrDefaultAsync();

        return product is null ? NotFound() : Ok(product);
    }
}

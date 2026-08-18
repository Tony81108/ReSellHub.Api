using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Controllers;

public class ProductManagementController : Controller
{
    private readonly AppDbContext _context;

    public ProductManagementController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
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
            .FirstOrDefaultAsync(item => item.Id == id);

        return product is null ? NotFound() : View(product);
    }

    public IActionResult Create()
    {
        LoadCategories();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            LoadCategories(product.CategoryId);
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "商品新增成功。";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        LoadCategories(product?.CategoryId);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            LoadCategories(product.CategoryId);
            return View(product);
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct is null)
        {
            return NotFound();
        }

        existingProduct.CategoryId = product.CategoryId;
        existingProduct.Name = product.Name;
        existingProduct.CostPrice = product.CostPrice;
        existingProduct.SellingPrice = product.SellingPrice;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.Condition = product.Condition;
        existingProduct.CoverImageUrl = product.CoverImageUrl;
        existingProduct.Description = product.Description;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "商品更新成功。";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "商品刪除成功。";
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadCategories(int? selectedId = null)
    {
        ViewBag.Categories = new SelectList(
            _context.Categories.Where(category => category.IsActive).OrderBy(category => category.SortOrder),
            "Id", "Name", selectedId);
    }
}

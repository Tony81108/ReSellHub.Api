using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartApiController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await GetOrCreateCartAsync();
        return Ok(await BuildCartResponseAsync(cart.Id));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        if (request.Quantity < 1)
        {
            return BadRequest(new { message = "商品數量至少為 1。" });
        }

        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive);
        if (product is null) return NotFound(new { message = "找不到商品。" });
        if (product.StockQuantity < request.Quantity) return BadRequest(new { message = "商品庫存不足。" });

        var cart = await GetOrCreateCartAsync();
        var item = await _context.CartItems.FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductId == request.ProductId);
        var newQuantity = (item?.Quantity ?? 0) + request.Quantity;
        if (newQuantity > product.StockQuantity) return BadRequest(new { message = $"庫存僅剩 {product.StockQuantity} 件。" });

        if (item is null)
        {
            _context.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = product.Id, Quantity = request.Quantity, AddedAt = DateTime.UtcNow });
        }
        else
        {
            item.Quantity = newQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(await BuildCartResponseAsync(cart.Id));
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<IActionResult> UpdateItem(int itemId, UpdateCartItemRequest request)
    {
        if (request.Quantity < 1) return BadRequest(new { message = "商品數量至少為 1。" });

        var cart = await GetOrCreateCartAsync();
        var item = await _context.CartItems.Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == itemId && x.CartId == cart.Id);
        if (item is null) return NotFound(new { message = "找不到購物車商品。" });
        if (request.Quantity > item.Product.StockQuantity) return BadRequest(new { message = $"庫存僅剩 {item.Product.StockQuantity} 件。" });

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(await BuildCartResponseAsync(cart.Id));
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var cart = await GetOrCreateCartAsync();
        var item = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == itemId && x.CartId == cart.Id);
        if (item is null) return NotFound(new { message = "找不到購物車商品。" });

        _context.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(await BuildCartResponseAsync(cart.Id));
    }

    private async Task<ShoppingCart> GetOrCreateCartAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart is not null) return cart;

        cart = new ShoppingCart { UserId = userId, UpdatedAt = DateTime.UtcNow };
        _context.ShoppingCarts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private async Task<object> BuildCartResponseAsync(int cartId)
    {
        var items = await _context.CartItems.AsNoTracking().Where(x => x.CartId == cartId)
            .OrderByDescending(x => x.AddedAt)
            .Select(x => new
            {
                x.Id, x.ProductId, x.Quantity,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.SellingPrice,
                StockQuantity = x.Product.StockQuantity,
                ImageUrl = x.Product.CoverImageUrl,
                LineTotal = x.Product.SellingPrice * x.Quantity
            }).ToListAsync();

        return new
        {
            items,
            itemCount = items.Sum(x => x.Quantity),
            subtotal = items.Sum(x => x.LineTotal),
            shippingFee = items.Count == 0 ? 0 : 60,
            total = items.Sum(x => x.LineTotal) + (items.Count == 0 ? 0 : 60)
        };
    }
}

public record AddCartItemRequest(int ProductId, int Quantity = 1);
public record UpdateCartItemRequest(int Quantity);

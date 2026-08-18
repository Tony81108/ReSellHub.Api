using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Controllers;

[ApiController, Authorize, Route("api/checkout")]
public class CheckoutApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public CheckoutApiController(AppDbContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Address))
            return BadRequest(new { message = "請完整填寫收件資料。" });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var cart = await _context.ShoppingCarts.Include(x => x.Items).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart is null || cart.Items.Count == 0) return BadRequest(new { message = "購物車是空的。" });
        foreach (var item in cart.Items)
            if (!item.Product.IsActive || item.Quantity > item.Product.StockQuantity) return BadRequest(new { message = $"「{item.Product.Name}」庫存不足。" });

        var subtotal = cart.Items.Sum(x => x.Product.SellingPrice * x.Quantity);
        var order = new Order
        {
            OrderNumber = $"RH{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..28], UserId = userId,
            Status = "Pending", PaymentStatus = "Unpaid", ShippingStatus = "Preparing",
            Subtotal = subtotal, ShippingFee = 60, TotalAmount = subtotal + 60,
            RecipientName = request.RecipientName.Trim(), RecipientPhone = request.Phone.Trim(), ShippingAddress = request.Address.Trim(), CreatedAt = DateTime.UtcNow
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        foreach (var item in cart.Items)
        {
            _context.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = item.ProductId, ProductName = item.Product.Name, UnitPrice = item.Product.SellingPrice, Quantity = item.Quantity, LineTotal = item.Product.SellingPrice * item.Quantity });
            item.Product.StockQuantity -= item.Quantity;
        }
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { order.Id, order.OrderNumber, order.TotalAmount });
    }
}

public record CheckoutRequest(string RecipientName, string Phone, string Address);

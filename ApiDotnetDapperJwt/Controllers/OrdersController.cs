using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // POST /orders → crear una orden con items (requiere autenticación)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // 👉 ahora usamos la función Postgres
        var orderId = await _unitOfWork.Orders.CreateOrderUsingFunctionAsync(userId, dto.Items);
        await _unitOfWork.SaveAsync();

        return Ok(new { orderId });
    }

    // GET /orders/{id} → obtiene detalles de una orden (con items)
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(id);
        if (order == null) return NotFound();

        var response = new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Total = order.Total,
            Items = order.OrderItems?.Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList() ?? new List<OrderItemResponseDto>()
        };

        return Ok(response);
    }

    // GET /orders → lista todas las órdenes del usuario autenticado
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(userId);

        var response = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            UserId = o.UserId,
            Total = o.Total
        });

        return Ok(response);
    }
}
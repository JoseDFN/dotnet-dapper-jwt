using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // POST /orders → crear una orden con items (requiere autenticación)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        _logger.LogInformation("Order creation attempt");

        // Check if model validation passed
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
            throw new ValidationException(errors);
        }

        // Obtener userId del token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Order creation attempt with invalid user authentication");
            throw new UnauthorizedException("Invalid user authentication");
        }

        // Verificar que el usuario existe
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Order creation attempt for non-existent user: {UserId}", userId);
            throw new NotFoundException("User", userId);
        }

        _logger.LogInformation("Creating order for user: {UserId} with {ItemCount} items", userId, dto.Items.Count());

        // Crear la orden usando la función Postgres
        var orderId = await _unitOfWork.Orders.CreateOrderUsingFunctionAsync(userId, dto.Items);
        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Order created successfully with ID: {OrderId} for user: {UserId}", orderId, userId);
        return Ok(new { orderId, message = "Order created successfully" });
    }

    // GET /orders/{id} → obtiene detalles de una orden (con items)
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int id)
    {
        // Validaciones de entrada
        if (id <= 0)
            throw new ValidationException("Id", "Order ID must be greater than 0");

        // Obtener userId del token para verificar autorización
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedException("Invalid user authentication");

        _logger.LogInformation("User {UserId} attempting to view order {OrderId}", userId, id);

        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(id);
        if (order == null)
            throw new NotFoundException("Order", id);

        _logger.LogInformation("Order {OrderId} found with UserId {OrderUserId}, requesting user {RequestingUserId}", 
            order.Id, order.UserId, userId);

        // Verificar que el usuario solo puede ver sus propias órdenes
        if (order.UserId != userId)
            throw new UnauthorizedException("You can only view your own orders");

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
        // Obtener userId del token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedException("Invalid user authentication");

        // Verificar que el usuario existe
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

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
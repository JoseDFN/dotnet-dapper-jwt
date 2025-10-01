using Application.DTOs.Products;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET /products?category=ropa&name=camisa
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? name)
    {
        var products = await _unitOfWork.Products.GetAllAsync(category, name);

        var response = products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name!,
            Sku = p.Sku!,
            Price = p.Price,
            Stock = p.Stock,
            Category = p.Category
        });

        return Ok(response);
    }

    // POST /products → requiere rol Admin
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Sku = dto.Sku,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var id = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveAsync();

        return CreatedAtAction(nameof(GetAll), new { id }, product);
    }

    // PUT /products/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var existing = await _unitOfWork.Products.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = dto.Name;
        existing.Sku = dto.Sku;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.Category = dto.Category;
        existing.updated_at = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(existing);
        await _unitOfWork.SaveAsync();

        return Ok(new { message = $"Producto {id} actualizado" });
    }

    // DELETE /products/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _unitOfWork.Products.GetByIdAsync(id);
        if (existing == null) return NotFound();

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveAsync();

        return Ok(new { message = $"Producto {id} eliminado" });
    }
}
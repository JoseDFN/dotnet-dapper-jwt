using Application.DTOs.Products;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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
        // Validaciones
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Name", "Product name is required");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new ValidationException("Sku", "Product SKU is required");

        if (dto.Price <= 0)
            throw new ValidationException("Price", "Product price must be greater than 0");

        if (dto.Stock < 0)
            throw new ValidationException("Stock", "Product stock cannot be negative");

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

        try
        {
            var id = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetAll), new { id }, product);
        }
        catch (Exception ex) when (ex.InnerException?.Message?.Contains("duplicate key value violates unique constraint") == true ||
                                   ex.InnerException?.Message?.Contains("unique constraint") == true ||
                                   ex.Message.Contains("duplicate key value violates unique constraint") ||
                                   ex.Message.Contains("unique constraint"))
        {
            throw new BusinessException("DUPLICATE_SKU", $"A product with SKU '{dto.Sku}' already exists. Please use a different SKU.");
        }
    }

    // PUT /products/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (id <= 0)
            throw new ValidationException("Id", "Product ID must be greater than 0");

        // Validaciones
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Name", "Product name is required");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new ValidationException("Sku", "Product SKU is required");

        if (dto.Price <= 0)
            throw new ValidationException("Price", "Product price must be greater than 0");

        if (dto.Stock < 0)
            throw new ValidationException("Stock", "Product stock cannot be negative");

        var existing = await _unitOfWork.Products.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Product", id);

        existing.Name = dto.Name;
        existing.Sku = dto.Sku;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.Category = dto.Category;
        existing.updated_at = DateTime.UtcNow;

        try
        {
            await _unitOfWork.Products.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = $"Producto {id} actualizado" });
        }
        catch (Exception ex) when (ex.InnerException?.Message?.Contains("duplicate key value violates unique constraint") == true ||
                                   ex.InnerException?.Message?.Contains("unique constraint") == true ||
                                   ex.Message.Contains("duplicate key value violates unique constraint") ||
                                   ex.Message.Contains("unique constraint"))
        {
            throw new BusinessException("DUPLICATE_SKU", $"A product with SKU '{dto.Sku}' already exists. Please use a different SKU.");
        }
    }

    // DELETE /products/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            throw new ValidationException("Id", "Product ID must be greater than 0");

        var existing = await _unitOfWork.Products.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Product", id);

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveAsync();

        return Ok(new { message = $"Producto {id} eliminado" });
    }
}
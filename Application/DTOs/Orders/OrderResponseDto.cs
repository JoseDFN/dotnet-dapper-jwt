using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Orders
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
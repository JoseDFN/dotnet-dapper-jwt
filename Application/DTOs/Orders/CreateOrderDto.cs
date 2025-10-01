using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Orders
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
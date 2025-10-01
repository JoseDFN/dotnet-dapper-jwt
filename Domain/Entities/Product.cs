using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Sku { get; set; }
        public Decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Category { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
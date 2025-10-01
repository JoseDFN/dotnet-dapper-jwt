using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
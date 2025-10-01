using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public abstract class BaseEntity
    {
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

    }
}
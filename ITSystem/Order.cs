using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem
{
    internal class Order
    {
        public int Id { get; set; }

        [Required, MaxLength(128)] public string CustomerName { get; set; } = string.Empty;
        [Required, MaxLength(128)] public string Item { get; set; } = string.Empty;

        [Range(1, 10000)] public int Quantity { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.New;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}

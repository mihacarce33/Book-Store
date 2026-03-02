using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string CustomerId { get; set; }  

        [Required]
        public float TotalAmount { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public void CalculateTotal()
        {
            if (OrderItems != null && OrderItems.Any())
            {
                TotalAmount = OrderItems.Sum(item => item.Quantity * item.Price);
            }
        }
    }

}
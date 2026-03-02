using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public string Name { get; set; }
        public string Biography { get; set; }
        public virtual ICollection<Book> Books { get; set; }
        public Author()
        {
            Books = new List<Book>();
        }
    }
}
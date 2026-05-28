using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? ImageURL { get; set; }

        public int? AuthorId { get; set; }
        public virtual Author? Author { get; set; }

        [Required]
        public int? GenreId { get; set; }
        public virtual Genre? Genre { get; set; }

        public float? Price { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int Stock { get; set; }
    }
}
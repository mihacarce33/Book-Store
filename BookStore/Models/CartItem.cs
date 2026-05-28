namespace BookStore.Models;

public class CartItem
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public float Price { get; set; }
    public int Quantity { get; set; }
}
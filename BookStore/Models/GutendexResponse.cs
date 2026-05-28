namespace BookStore.Models;

public class GutendexResponse
{
    public int? count { get; set; }
    public string? next { get; set; }
    public string? previous { get; set; }
    public List<GutendexBook>? results { get; set; }
}
using System.Text.Json;
using BookStore.Models;

namespace BookStore.Services;

public class CartService : ICartService
{
    private const string CartSessionKey = "BookStoreCart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItem> GetCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return new List<CartItem>();
        }

        var json = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    public void SaveCart(List<CartItem> cart)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }

    public int GetItemCount() => GetCart().Sum(item => item.Quantity);
}

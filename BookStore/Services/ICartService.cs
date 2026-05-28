using BookStore.Models;

namespace BookStore.Services;

public interface ICartService
{
    List<CartItem> GetCart();
    void SaveCart(List<CartItem> cart);
    int GetItemCount();
}

using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookStore.Controllers;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;

    public BooksController(ApplicationDbContext db, ICartService cartService)
    {
        _db = db;
        _cartService = cartService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? genreId, string? priceSort)
    {
        var books = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .AsQueryable();

        if (genreId.HasValue)
        {
            books = books.Where(b => b.GenreId == genreId.Value);
        }

        books = priceSort switch
        {
            "asc" => books.OrderBy(b => b.Price),
            "desc" => books.OrderByDescending(b => b.Price),
            _ => books.OrderBy(b => b.Title)
        };

        ViewBag.Genres = new SelectList(await _db.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId);
        return View(await books.ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var book = await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateBookDropdownsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Id,Title,ImageURL,Price,Description,Stock,AuthorId,GenreId")] Book book)
    {
        if (ModelState.IsValid)
        {
            _db.Books.Add(book);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateBookDropdownsAsync(book.AuthorId, book.GenreId);
        return View(book);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FetchFromApi()
    {
        using var client = new HttpClient();
        string? apiUrl = "https://gutendex.com/books";
        int booksFetched = 0;
        int maxNumberOfBooks = 20;

        while (!string.IsNullOrEmpty(apiUrl))
        {
            if (maxNumberOfBooks == 0)
            {
                break;
            }

            var response = await client.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GutendexResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result?.results == null)
            {
                break;
            }

            foreach (var apiBook in result.results)
            {
                if (maxNumberOfBooks == 0)
                {
                    break;
                }

                if (await _db.Books.AnyAsync(b => b.Title == apiBook.title))
                {
                    continue;
                }

                var firstAuthor = apiBook.authors?.FirstOrDefault();
                string authorName = firstAuthor?.name ?? "Unknown";
                var authorBirthYear = firstAuthor?.birth_year;
                var authorDeathYear = firstAuthor?.death_year;
                string authorBiography = authorBirthYear != null && authorDeathYear != null
                    ? $"Birth Year: {authorBirthYear}\nDeath Year: {authorDeathYear}"
                    : authorBirthYear != null
                        ? $"Birth Year: {authorBirthYear}"
                        : authorDeathYear != null
                            ? $"Death Year: {authorDeathYear}"
                            : string.Empty;

                var author = await _db.Authors.FirstOrDefaultAsync(a => a.Name == authorName);
                if (author == null)
                {
                    author = new Author { Name = authorName, Biography = authorBiography };
                    _db.Authors.Add(author);
                    await _db.SaveChangesAsync();
                }

                string genreName = apiBook.subjects?.FirstOrDefault() ?? "Uncategorized";
                var genre = await _db.Genres.FirstOrDefaultAsync(g => g.Name == genreName);
                if (genre == null)
                {
                    genre = new Genre { Name = genreName };
                    _db.Genres.Add(genre);
                    await _db.SaveChangesAsync();
                }

                string description = apiBook.summaries?.FirstOrDefault() ?? "No description available.";
                string? imageUrl = apiBook.formats != null && apiBook.formats.TryGetValue("image/jpeg", out var url) ? url : null;

                var newBook = new Book
                {
                    Title = apiBook.title?.Contains("$b") == true
                        ? apiBook.title.Split(new[] { "$b" }, StringSplitOptions.None)[0].Trim()
                        : apiBook.title?.Trim() ?? "Untitled",
                    ImageURL = imageUrl,
                    Description = description,
                    AuthorId = author.Id,
                    GenreId = genre.Id,
                    Price = null,
                    Stock = 0
                };

                _db.Books.Add(newBook);
                booksFetched++;
                maxNumberOfBooks--;
            }

            await _db.SaveChangesAsync();
            apiUrl = result.next;
        }

        TempData["Message"] = $"{booksFetched} books imported from Gutendex.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var book = await _db.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        await PopulateBookDropdownsAsync(book.AuthorId, book.GenreId);
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImageURL,Price,Description,Stock,AuthorId,GenreId")] Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Update(book);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateBookDropdownsAsync(book.AuthorId, book.GenreId);
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book != null)
        {
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet]
    public async Task<IActionResult> AddToCart(int id, int quantity = 1)
    {
        return await AddToCartInternal(id, quantity, redirectToCart: false);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<IActionResult> AddToCartPost(int id, int quantity)
    {
        return await AddToCartInternal(id, quantity, redirectToCart: true);
    }

    [Authorize(Roles = "Admin,User")]
    public IActionResult ViewCart()
    {
        return View("Cart", _cartService.GetCart());
    }

    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> RemoveFromCart(int id, int quantity)
    {
        var cart = _cartService.GetCart();
        var cartItem = cart.FirstOrDefault(c => c.BookId == id);
        if (cartItem != null)
        {
            if (quantity >= cartItem.Quantity)
            {
                cart.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity -= quantity;
            }
        }

        _cartService.SaveCart(cart);

        var book = await _db.Books.FindAsync(id);
        if (book != null)
        {
            book.Stock += quantity;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ViewCart));
    }

    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Checkout()
    {
        var cart = _cartService.GetCart();
        if (!cart.Any())
        {
            TempData["Message"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        var customerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.Now,
            OrderItems = cart.Select(c => new OrderItem
            {
                BookId = c.BookId,
                Quantity = c.Quantity,
                Price = c.Price
            }).ToList()
        };
        order.CalculateTotal();

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _cartService.SaveCart(new List<CartItem>());

        TempData["Message"] = $"Your order has been placed! Total: {order.TotalAmount:C}. Thank you for your purchase!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> AddToCartInternal(int id, int quantity, bool redirectToCart)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            TempData["Message"] = "Please select a valid quantity.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity > book.Stock)
        {
            TempData["Message"] = "Sorry, we don't have enough stock for this book.";
            return RedirectToAction(nameof(Index));
        }

        var cart = _cartService.GetCart();
        var cartItem = cart.FirstOrDefault(c => c.BookId == id);
        if (cartItem == null)
        {
            cart.Add(new CartItem
            {
                BookId = id,
                Title = book.Title,
                Price = book.Price ?? 0,
                Quantity = quantity,
                ImageUrl = book.ImageURL
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        _cartService.SaveCart(cart);
        book.Stock -= quantity;
        await _db.SaveChangesAsync();

        if (redirectToCart)
        {
            return RedirectToAction(nameof(ViewCart));
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateBookDropdownsAsync(int? authorId = null, int? genreId = null)
    {
        ViewBag.Authors = new SelectList(await _db.Authors.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", authorId);
        ViewBag.Genres = new SelectList(await _db.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId);
    }
}

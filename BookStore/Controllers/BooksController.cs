using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using Microsoft.AspNet.Identity;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Books
        [AllowAnonymous]
        public ActionResult Index(int? genreId, string priceSort)
        {
            var books = db.Books.AsQueryable();

            if (genreId.HasValue)
            {
                books = books.Where(b => b.GenreId == genreId.Value);
            }

            if (priceSort == "asc")
            {
                books = books.OrderBy(b => b.Price);
            }
            else if (priceSort == "desc")
            {
                books = books.OrderByDescending(b => b.Price);
            }

            var genres = new SelectList(db.Genres, "Id", "Name");
            ViewBag.Genres = genres;

            return View(books.ToList());
        }



        // GET: Books/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Authors = new SelectList(db.Authors, "Id", "Name");
            ViewBag.Genres = new SelectList(db.Genres, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,ImageURL,Price,Description,Stock,AuthorId,GenreId")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Authors = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            ViewBag.Genres = new SelectList(db.Genres, "Id", "Name", book.GenreId);

            return View(book);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> FetchFromApi()
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                string apiUrl = "https://gutendex.com/books";
                int booksFetched = 0;
                int maxNumberOfBooks = 20;

                while (!string.IsNullOrEmpty(apiUrl))
                {
                    if (maxNumberOfBooks == 0)
                        break;

                    var response = await client.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode) break;

                    var json = await response.Content.ReadAsStringAsync();
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<GutendexResponse>(json);
                    foreach (var apiBook in result.results)
                    {
                        if (maxNumberOfBooks == 0)
                            break;
                        
                        if (db.Books.Any(b => b.Title == apiBook.title))
                            continue;

                        
                        string authorName = apiBook.authors.FirstOrDefault()?.name ?? "Unknown";
                        var authorBirthYear = apiBook.authors.FirstOrDefault()?.birth_year ?? null;
                        var authorDeathYear = apiBook.authors.FirstOrDefault()?.death_year ?? null;
                        string authorBiography = "";
                        if (authorBirthYear != null && authorDeathYear != null)
                        {
                            authorBiography = "Birth Year: " + authorBirthYear + "\nDeath Year: " + authorDeathYear;
                        } else if (authorBirthYear != null && authorDeathYear == null)
                        {
                            authorBiography = "Birth Year: " + authorBirthYear;
                        } else if (authorDeathYear != null && authorBirthYear == null)
                        {
                            authorBiography = "Death Year: " + authorBirthYear;
                        }
                        var author = db.Authors.FirstOrDefault(a => a.Name == authorName);
                        if (author == null)
                        {
                            author = new Author { 
                                Name = authorName,
                                Biography = authorBiography,
                            };
                            db.Authors.Add(author);
                        }

                        // Get or create genre
                        string genreName = apiBook.subjects.FirstOrDefault() ?? "Uncategorized";
                        var genre = db.Genres.FirstOrDefault(g => g.Name == genreName);
                        if (genre == null)
                        {
                            genre = new Genre { Name = genreName };
                            db.Genres.Add(genre);
                            db.SaveChanges();
                        }

                        // Description
                        string description = apiBook.summaries?.FirstOrDefault() ?? "No description available.";

                        // Image
                        string imageUrl = apiBook.formats.ContainsKey("image/jpeg") ? apiBook.formats["image/jpeg"] : null;

                        // Create and add book
                        var newBook = new Book
                        {
                            Title = apiBook.title.Contains("$b")
                                ? apiBook.title.Split(new[] { "$b" }, StringSplitOptions.None)[0].Trim()
                                : apiBook.title.Trim(),
                            ImageURL = imageUrl,
                            Description = description,
                            AuthorId = author.Id,
                            GenreId = genre.Id,
                            Price = null,
                            Stock = 0
                        };

                        db.Books.Add(newBook);
                        booksFetched++;

                        author.Books.Add(newBook);

                        maxNumberOfBooks--;
                    }

                    db.SaveChanges();
                    apiUrl = result.next;
                }

                TempData["Message"] = $"{booksFetched} books imported from Gutendex.";
                return RedirectToAction("Index");
            }
        }


        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            ViewBag.Authors = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            ViewBag.Genres = new SelectList(db.Genres, "Id", "Name", book.GenreId);

            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,ImageURL,Price,Description,Stock,AuthorId,GenreId")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Authors = new SelectList(db.Authors, "Id", "Name", book.AuthorId);
            ViewBag.Genres = new SelectList(db.Genres, "Id", "Name", book.GenreId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Books/Buy/5
        [Authorize(Roles = "Admin, User")]
        public ActionResult Buy(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            if (book.Stock > 0)
            {
                book.Stock--;  
                db.SaveChanges();
                TempData["Message"] = "Purchase successful!";
            }
            else
            {
                TempData["Message"] = "Sorry, this book is out of stock.";
            }

            return RedirectToAction("Index"); 
        }

        // Add to Cart Action
        [Authorize(Roles = "Admin, User")]
        public ActionResult AddToCart(int id, int quantity)
        {
            var book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            if (quantity > book.Stock)
            {
                TempData["Message"] = "Sorry, we don't have enough stock for this book.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();

            var cartItem = cart.FirstOrDefault(c => c.BookId == id);
            if (cartItem == null)
            {
                cart.Add(new CartItem { BookId = id, Title = book.Title, Price = book.Price ?? 0, Quantity = quantity, ImageUrl = book.ImageURL });
            }
            else
            {
                cartItem.Quantity += quantity;  
            }

            SaveCart(cart);

            book.Stock -= quantity;
            db.SaveChanges();

            return RedirectToAction("Index");
        }



        // View Cart Action
        [Authorize(Roles = "Admin, User")]
        public ActionResult ViewCart()
        {
            var cart = GetCart();
            return View("Cart", cart);
        }

        // Remove Item from Cart
        [Authorize(Roles = "Admin, User")]
        public ActionResult RemoveFromCart(int id, int quantity)
        {
            var cart = GetCart();
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

            SaveCart(cart);

            var book = db.Books.Find(id);
            if (book != null)
            {
                book.Stock += quantity; 
                db.SaveChanges();
            }

            return RedirectToAction("ViewCart");
        }

        [Authorize(Roles = "Admin, User")]
        public ActionResult Checkout()
        {
            var cart = GetCart();

            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            string customerId = User.Identity.GetUserId();

            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    BookId = cartItem.BookId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price
                };

                order.OrderItems.Add(orderItem);
            }

            order.CalculateTotal();

            db.Orders.Add(order);
            db.SaveChanges();

            SaveCart(new List<CartItem>());

            TempData["Message"] = $"Your order has been placed! Total: ${order.TotalAmount}. Thank you for your purchase!";
            return RedirectToAction("Index");
        }


        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            return cart;
        }

        private void SaveCart(List<CartItem> cart)
        {
            Session["Cart"] = cart;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

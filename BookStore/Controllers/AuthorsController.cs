using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class AuthorsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AuthorsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await _db.Authors.OrderBy(a => a.Name).ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var author = await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        ViewBag.Books = new SelectList(_db.Books.Where(b => b.AuthorId == null), "Id", "Title");
        return View(author);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Id,ImageUrl,Name,Biography")] Author author)
    {
        if (ModelState.IsValid)
        {
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(author);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var author = await _db.Authors.FindAsync(id);
        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ImageUrl,Name,Biography")] Author author)
    {
        if (id != author.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Update(author);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author != null)
        {
            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBook(int? id)
    {
        if (id == null)
        {
            return BadRequest();
        }

        var author = await _db.Authors.FindAsync(id);
        if (author == null)
        {
            return NotFound();
        }

        ViewBag.Books = new SelectList(_db.Books.Where(b => b.AuthorId == null), "Id", "Title");
        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBook(int id, int bookId)
    {
        var author = await _db.Authors.FindAsync(id);
        var book = await _db.Books.FindAsync(bookId);
        if (author == null || book == null)
        {
            return NotFound();
        }

        book.AuthorId = author.Id;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = author.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveBook(int id, int bookId)
    {
        var author = await _db.Authors.FindAsync(id);
        var book = await _db.Books.FindAsync(bookId);
        if (author == null || book == null)
        {
            return NotFound();
        }

        book.AuthorId = null;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = author.Id });
    }
}

using BookStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

[Authorize(Roles = "Admin")]
public class OrderItemsController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrderItemsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.OrderItems
            .Include(oi => oi.Book)
            .Include(oi => oi.Order)
            .ToListAsync());
    }
}

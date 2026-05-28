using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult About()
    {
        ViewBag.Message = "Your trusted destination for great books.";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Contact()
    {
        ViewBag.Message = "We are here to help with orders, recommendations, and more.";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}

using Dcd.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dcd.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(Portfolio.Content);
}

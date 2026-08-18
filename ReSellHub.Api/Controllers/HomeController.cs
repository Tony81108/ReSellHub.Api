using Microsoft.AspNetCore.Mvc;

namespace ReSellHub.Api.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Redirect("/app/index.html");
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TransitFeeds.Controllers
{
    public class DocumentationController : Controller
    {
        // GET: /Documentation
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace GoogleTranslateHistoryAPI.Controllers
{
    public class SuccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

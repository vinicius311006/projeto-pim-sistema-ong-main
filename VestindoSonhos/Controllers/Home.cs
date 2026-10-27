using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador")]
    public class Home : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard", "Financeiro");
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction("Dashboard", "Financeiro");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

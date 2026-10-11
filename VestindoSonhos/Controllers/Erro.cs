using Microsoft.AspNetCore.Mvc;

namespace VestindoSonhos.Controllers
{
    public class Erro : Controller
    {
        public IActionResult AcessoNegado()
        {
            TempData["Erro"] = "Você não tem permissão para acessar essa funcionalidade.";
            return RedirectToAction(controllerName: "Home", actionName: "Index");
        }
        public IActionResult InventarioInvalida()
        {
            TempData["Erro"] = "Transação Inválida";
            return RedirectToAction(controllerName: "Inventario", actionName: "Index");
        }

        public IActionResult ContaNaoVazia()
        {
            TempData["Erro"] = "Inválido. Conta possui saldo diferente de zero.";
            return RedirectToAction(controllerName: "Carteiras", actionName: "Index");
        }
    }
}

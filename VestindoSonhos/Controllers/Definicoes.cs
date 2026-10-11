using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Definicoes : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Definicoes(VestindosonhosdbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: Definicoes/RedefinirSenha
        [Authorize(Roles = "Administrador,Colaborador")]
        public IActionResult RedefinirSenha()
        {
            return View();
        }

        // POST: Definicoes/RedefinirSenha
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha([Bind("SenhaAtual,SenhaNova,RepeteSenhaNova")] RedefinicaoViewModel model)
        {
            if (model.SenhaNova != model.RepeteSenhaNova)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "As novas senhas não coincidem");
                return View();
            }
            else if (model.SenhaNova.Length < 8)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "A senha é muito fraca, são necessários ao menos 8 caracteres.");
                return View();
            }
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                if (userId == null)
                {
                    TempData["Erro"] = "Realize o login primeiro ou contate o suporte.";
                    return RedirectToAction(controllerName: "Login", actionName: "Index");
                }
                var usuario = _context.Usuario.FirstOrDefault(u => u.Id == userId);
                var result = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, model.SenhaAtual);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(nameof(model.SenhaAtual), "Senha inválida.");
                    return View(model);
                }
                var senhaCriptografada = passwordHasher.HashPassword(usuario, model.SenhaNova);
                usuario.Senha = senhaCriptografada;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Senha alterada com sucesso.";
                return RedirectToAction(controllerName: "Home", actionName: "Index");
            }
            return View(model);
        }
    }
}

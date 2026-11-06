using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    [AllowAnonymous]
    public class Login : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Login(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: /Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Email", "Senha")] LoginViewModel model)
        {
            var passwordHasher = new PasswordHasher<Usuario>();
            var emailLimpo = model.Email?.Trim() ?? string.Empty;

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == emailLimpo);
            if (usuario == null)
            {
                usuario = await _context.Usuario.FirstOrDefaultAsync(u => EF.Functions.Like(u.Email, emailLimpo));
            }

            if (usuario == null)
            {
                ModelState.AddModelError(nameof(model.Senha), "Email ou senha inválidos.");
                return View(model);
            }

            // Temporario migração de senha explícitas
            if (!usuario.Senha.StartsWith("$") && !usuario.Senha.StartsWith("AQAAAA"))
            {
                if (usuario.Senha == model.Senha)
                {
                    usuario.Senha = passwordHasher.HashPassword(usuario, model.Senha);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Senha), "Email ou senha inválidos.");
                    return View(model);
                }
            }
            // Temp ^
            PasswordVerificationResult result;
            try
            {
                result = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, model.Senha);
            }
            catch
            {
                result = usuario.Senha == model.Senha ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
            }
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.Senha), "Email ou senha inválidos.");
                return View(model);
            }

            // Identifica o perfil (Cargo 1 = Administrador, 2 = Colaborador)
            string roleName = usuario.Cargo == 1 ? "Administrador" : "Colaborador";
            try
            {
                var cargo = await _context.Cargo.FindAsync(usuario.Cargo);
                if (!string.IsNullOrEmpty(cargo?.Nome))
                {
                    roleName = cargo.Nome;
                }
            }
            catch
            {
                roleName = usuario.Cargo == 1 ? "Administrador" : "Colaborador";
            }

            // Verifica se o administrador exigiu a troca de senha temporária
            if (usuario.ExigirTrocaSenha)
            {
                var claimsObrigatorias = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email ?? string.Empty),
                    new Claim("Nome", usuario.Nome ?? string.Empty),
                    new Claim("UserId", usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("ExigeTrocaSenha", "true")
                };

                var claimsIdentityObrigatoria = new ClaimsIdentity(
                    claimsObrigatorias, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentityObrigatoria));

                TempData["AvisoTroca"] = "Para sua segurança, defina uma nova senha pessoal antes de continuar.";
                return RedirectToAction(nameof(TrocarSenhaObrigatoria));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email ?? string.Empty),
                new Claim("Nome", usuario.Nome ?? string.Empty),
                new Claim("UserId", usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, roleName),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Dashboard", "Financeiro");
        }

        // GET: /Login/TrocarSenhaObrigatoria
        [AllowAnonymous]
        public async Task<IActionResult> TrocarSenhaObrigatoria()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Index));
            }

            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _context.Usuario.FindAsync(userId);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!usuario.ExigirTrocaSenha)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new TrocarSenhaObrigatoriaViewModel
            {
                Nome = usuario.Nome,
                Email = usuario.Email ?? string.Empty
            };

            return View(model);
        }

        // POST: /Login/TrocarSenhaObrigatoria
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrocarSenhaObrigatoria(TrocarSenhaObrigatoriaViewModel model)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Index));
            }

            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Index));
            }

            string roleName = usuario.Cargo == 1 ? "Administrador" : "Colaborador";
            try
            {
                var cargo = await _context.Cargo.FindAsync(usuario.Cargo);
                if (!string.IsNullOrEmpty(cargo?.Nome))
                {
                    roleName = cargo.Nome;
                }
            }
            catch
            {
                roleName = usuario.Cargo == 1 ? "Administrador" : "Colaborador";
            }

            // Não permite reutilizar senhas padrão conhecidas
            if (model.NovaSenha == "123456" || model.NovaSenha.Equals("Ong@2026", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.NovaSenha), "Por motivos de segurança, você não pode reutilizar a senha padrão inicial. Escolha uma nova senha pessoal.");
            }

            if (!ModelState.IsValid)
            {
                model.Nome = usuario.Nome;
                model.Email = usuario.Email ?? string.Empty;
                return View(model);
            }

            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Senha = passwordHasher.HashPassword(usuario, model.NovaSenha);
            usuario.ExigirTrocaSenha = false;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            // Re-autentica sem a claim ExigeTrocaSenha
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email ?? string.Empty),
                new Claim("Nome", usuario.Nome),
                new Claim("UserId", usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            TempData["Sucesso"] = "Sua nova senha foi cadastrada com sucesso! Bem-vindo ao sistema da ONG Vestindo Sonhos.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Login/RecuperarSenha
        [AllowAnonymous]
        public ActionResult RecuperarSenha()
        {
            TempData["Erro"] = "A alteração e redefinição de senhas é realizada exclusivamente pelo Administrador do sistema.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /RecuperarSenha
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarSenha([Bind("Email,Cpf")] RecuperarSenhaViewModel model)
        {
            TempData["Erro"] = "A alteração e redefinição de senhas é realizada exclusivamente pelo Administrador do sistema.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Login/DefinirNovaSenha
        [AllowAnonymous]
        public ActionResult DefinirNovaSenha(RecuperarSenhaViewModel model)
        {
            TempData["Erro"] = "A alteração e redefinição de senhas é realizada exclusivamente pelo Administrador do sistema.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Login/DefinirNovaSenha
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DefinirNovaSenha([Bind("Email,Cpf,SenhaNova,RepeteSenhaNova")] DefinirNovaSenhaViewModel model)
        {
            TempData["Erro"] = "A alteração e redefinição de senhas é realizada exclusivamente pelo Administrador do sistema.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Logout
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Index));

        }
    }
}

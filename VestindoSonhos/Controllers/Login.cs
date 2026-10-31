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
            var usuario = _context.Usuario.Include(u => u.CargoNavigation)
                .FirstOrDefault(u => u.Email.ToLower() == emailLimpo.ToLower());
            if(usuario == null)
            {
                ModelState.AddModelError(nameof(model.Senha), "Email ou senha inválidos.");
                return View(model);
            }
            // Temporario migração de senha explícitas
            if (!usuario.Senha.StartsWith("$"))
            {
                if (usuario.Senha == model.Senha)
                {
                    usuario.Senha = passwordHasher.HashPassword(usuario, model.Senha);
                    _context.SaveChanges();
                }
            }
            // Temp ^
            var result = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, model.Senha);
            if(result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.Senha), "Email ou senha inválidos.");
                return View(model);
            }

            // Verifica se o administrador exigiu a troca de senha temporária
            if (usuario.ExigirTrocaSenha)
            {
                var claimsObrigatorias = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email ?? string.Empty),
                    new Claim("Nome", usuario.Nome ?? string.Empty),
                    new Claim("UserId", usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, usuario.CargoNavigation?.Nome ?? "Administrador"),
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
                new Claim(ClaimTypes.Role, usuario.CargoNavigation?.Nome ?? "Administrador"),
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

            var usuario = await _context.Usuario.Include(u => u.CargoNavigation).FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Index));
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
                new Claim(ClaimTypes.Role, usuario.CargoNavigation?.Nome ?? "Usuário")
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
            return View();
        }

        // POST: /RecuperarSenha
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecuperarSenha([Bind("Email,Cpf")] RecuperarSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (usuario == null || usuario.Cpf != model.Cpf)
                {
                    TempData["Erro"] = "Usuário inválido.";
                    return RedirectToAction(controllerName: "Home", actionName: "Index");
                }
                return RedirectToAction(actionName: "DefinirNovaSenha", model);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Login/RecuperarSenha
        [AllowAnonymous]
        public ActionResult DefinirNovaSenha(RecuperarSenhaViewModel model)
        {
            return View();
        }

        // POST: /RecuperarSenha
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DefinirNovaSenha([Bind("Email,Cpf,SenhaNova,RepeteSenhaNova")] DefinirNovaSenhaViewModel model)
        {
            if (model.SenhaNova != model.RepeteSenhaNova)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "As novas senhas não coincidem");
                return View(model);
            }
            else if (model.SenhaNova.Length < 8)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "A senha é muito fraca, são necessários ao menos 8 caracteres.");
                return View(model);
            }
            //FOREACH para DEBUG
            foreach (var erro in ModelState)
            {
                foreach (var subErro in erro.Value.Errors)
                {
                    Console.WriteLine($"{erro.Key}: {subErro.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("|| Ok ||");
                var passwordHasher = new PasswordHasher<Usuario>();
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (usuario == null || usuario.Cpf != model.Cpf)
                {
                    TempData["Erro"] = "Usuário inválido.";
                    return RedirectToAction(controllerName: "Home", actionName: "Index");
                }
                var senhaCriptografada = passwordHasher.HashPassword(usuario, model.SenhaNova);
                usuario.Senha = senhaCriptografada;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Senha alterada com sucesso.";
                return RedirectToAction(controllerName: "Home", actionName: "Index");
            }
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

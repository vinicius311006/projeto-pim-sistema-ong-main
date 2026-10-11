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
            var usuario = _context.Usuario.Include(u => u.CargoNavigation).FirstOrDefault(u => u.Email == model.Email);
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

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.Email),
            new Claim("Nome", usuario.Nome),
            new Claim("UserId", usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, usuario.CargoNavigation.Nome),
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,

                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            

            return RedirectToAction(controllerName: nameof(Home), actionName: nameof(Index));
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

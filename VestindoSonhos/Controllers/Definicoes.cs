using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using VestindoSonhos.Helpers;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class Definicoes : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Definicoes(VestindosonhosdbContext context)
        {
            _context = context;
        }

        private SaasConfigModel CarregarConfiguracoes()
        {
            return ConfiguracaoSistemaHelper.ObterConfiguracoes();
        }

        public async Task<IActionResult> Index()
        {
            var config = CarregarConfiguracoes();

            var userIdStr = User.FindFirst("UserId")?.Value;
            Usuario? usuarioLogado = null;
            if (int.TryParse(userIdStr, out int uid))
            {
                usuarioLogado = await _context.Usuario
                    .Include(u => u.CargoNavigation)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == uid);
            }

            ViewBag.Config = config;
            ViewBag.UsuarioNome = usuarioLogado?.Nome ?? User.FindFirst("Nome")?.Value ?? "Administrador";
            ViewBag.UsuarioEmail = usuarioLogado?.Email ?? User.Identity?.Name ?? "admin@teste.org";
            ViewBag.UsuarioCargo = usuarioLogado?.CargoNavigation?.Nome ?? "Administrador";

            // Contadores dos módulos e cadastros base
            ViewBag.TotalCarteiras = await _context.Carteira.CountAsync();
            ViewBag.SaldoTotalCarteiras = await _context.Carteira.SumAsync(c => (decimal?)c.Saldo) ?? 0m;
            ViewBag.TotalCategoriasFin = await _context.Categoria.CountAsync();
            ViewBag.TotalTiposAtivo = await _context.TiposAtivo.CountAsync();
            ViewBag.TotalProdutos = await _context.Produto.CountAsync();
            ViewBag.TotalCargos = await _context.Cargo.CountAsync();
            ViewBag.TotalUsuarios = await _context.Usuario.CountAsync();
            ViewBag.TotalBeneficiarios = await _context.Beneficiario.CountAsync();
            ViewBag.TotalVoluntarios = await _context.Voluntario.CountAsync();
            ViewBag.TotalEventos = await _context.Evento.CountAsync();

            bool dbOnline = false;
            try
            {
                dbOnline = await _context.Database.CanConnectAsync();
            }
            catch { }
            ViewBag.DbOnline = dbOnline;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarConfiguracoes(SaasConfigModel model)
        {
            try
            {
                ConfiguracaoSistemaHelper.SalvarConfiguracoes(model);
                TempData["Sucesso"] = "Configurações do sistema atualizadas com sucesso! As alterações já estão ativas no site.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Não foi possível salvar as configurações: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportarBackupJson()
        {
            var snapshot = new
            {
                DataExportacao = DateTime.Now,
                Sistema = "ONG Vestindo Sonhos - SaaS Export",
                Configuracoes = CarregarConfiguracoes(),
                Resumo = new
                {
                    Usuarios = await _context.Usuario.CountAsync(),
                    Beneficiarios = await _context.Beneficiario.CountAsync(),
                    Dependentes = await _context.Dependente.CountAsync(),
                    Voluntarios = await _context.Voluntario.CountAsync(),
                    Eventos = await _context.Evento.CountAsync(),
                    Produtos = await _context.Produto.CountAsync(),
                    Carteiras = await _context.Carteira.AsNoTracking().ToListAsync(),
                    CategoriasFinanceiras = await _context.Categoria.AsNoTracking().ToListAsync(),
                    TiposAtivo = await _context.TiposAtivo.AsNoTracking().ToListAsync()
                }
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot, new JsonSerializerOptions { WriteIndented = true });
            return File(bytes, "application/json", $"backup-vestindo-sonhos-{DateTime.Now:yyyyMMdd-HHmm}.json");
        }

        // GET: Definicoes/RedefinirSenha
        public IActionResult RedefinirSenha()
        {
            return View();
        }

        // POST: Definicoes/RedefinirSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha([Bind("SenhaAtual,SenhaNova,RepeteSenhaNova")] RedefinicaoViewModel model)
        {
            if (model.SenhaNova != model.RepeteSenhaNova)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "As novas senhas não coincidem");
                return View(model);
            }
            else if (string.IsNullOrEmpty(model.SenhaNova) || model.SenhaNova.Length < 6)
            {
                ModelState.AddModelError(nameof(model.SenhaNova), "A senha deve possuir no mínimo 6 caracteres.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    TempData["Erro"] = "Realize o login primeiro ou contate o suporte.";
                    return RedirectToAction("Index", "Login");
                }

                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == userId);
                if (usuario == null)
                {
                    TempData["Erro"] = "Usuário não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var result = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, model.SenhaAtual);
                if (result == PasswordVerificationResult.Failed && usuario.Senha != model.SenhaAtual)
                {
                    ModelState.AddModelError(nameof(model.SenhaAtual), "Senha atual inválida.");
                    return View(model);
                }

                usuario.Senha = passwordHasher.HashPassword(usuario, model.SenhaNova);
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Sua senha administrativa foi alterada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers
{
    public class Categoria : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Categoria(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Categoria
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoria.ToListAsync());
        }

        // GET: Categoria/Detalhes/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categoria/Criar
        [Authorize(Roles = "Administrador,Colaborador")]
        public IActionResult Criar(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Categoria/Criar
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id", "Nome")] Models.Categoria categoria, string? returnUrl = null)
        {
            categoria.Nome = categoria.Nome?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(categoria.Nome))
            {
                ModelState.AddModelError(nameof(categoria.Nome), "O nome da categoria é obrigatório.");
            }
            else if (categoria.Nome.Length > 60)
            {
                categoria.Nome = categoria.Nome.Substring(0, 60);
            }

            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();

                bool urlLocalSegura = !string.IsNullOrWhiteSpace(returnUrl) &&
                    (Url != null
                        ? Url.IsLocalUrl(returnUrl)
                        : (returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\")));

                if (urlLocalSegura)
                {
                    return LocalRedirect(returnUrl!);
                }

                if (User?.IsInRole("Administrador") == true)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction("Index", "Inventario");
            }

            return View(categoria);
        }

        // GET: Categoria/Editar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categoria/Editar/5
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome")] Models.Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return NotFound();
            }

            categoria.Nome = categoria.Nome?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(categoria.Nome))
            {
                ModelState.AddModelError(nameof(categoria.Nome), "O nome da categoria é obrigatório.");
            }
            else if (categoria.Nome.Length > 60)
            {
                categoria.Nome = categoria.Nome.Substring(0, 60);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categoria/Apagar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categoria/Apagar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);
                if (categoria != null)
                {
                    // Desvincula as transações existentes (deixando-as Sem Categoria) para permitir apagar inclusive a última categoria
                    var vinculadas = await _context.Transacao.Where(t => t.Categoria == id).ToListAsync();
                    if (vinculadas.Any())
                    {
                        foreach (var t in vinculadas)
                        {
                            t.Categoria = null;
                        }
                        _context.Transacao.UpdateRange(vinculadas);
                        await _context.SaveChangesAsync();
                    }

                    _context.Categoria.Remove(categoria);
                    await _context.SaveChangesAsync();
                    if (TempData != null)
                    {
                        TempData["Sucesso"] = "Categoria financeira excluída com sucesso!";
                    }
                }
            }
            catch (Exception)
            {
                if (TempData != null)
                {
                    TempData["Erro"] = "Não foi possível excluir esta categoria financeira.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Categoria/ApagarTodas
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarTodas(string? returnUrl = null)
        {
            try
            {
                var vinculadas = await _context.Transacao.Where(t => t.Categoria != null).ToListAsync();
                if (vinculadas.Any())
                {
                    foreach (var t in vinculadas)
                    {
                        t.Categoria = null;
                    }
                    _context.Transacao.UpdateRange(vinculadas);
                    await _context.SaveChangesAsync();
                }

                var todasCategorias = await _context.Categoria.ToListAsync();
                if (todasCategorias.Any())
                {
                    _context.Categoria.RemoveRange(todasCategorias);
                    await _context.SaveChangesAsync();
                }

                if (TempData != null)
                {
                    TempData["Sucesso"] = "Todas as categorias financeiras foram excluídas com sucesso!";
                }
            }
            catch (Exception)
            {
                if (TempData != null)
                {
                    TempData["Erro"] = "Não foi possível excluir todas as categorias financeiras.";
                }
            }

            bool urlLocalSegura = !string.IsNullOrWhiteSpace(returnUrl) &&
                (Url != null
                    ? Url.IsLocalUrl(returnUrl)
                    : (returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\")));

            if (urlLocalSegura)
            {
                return LocalRedirect(returnUrl!);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}

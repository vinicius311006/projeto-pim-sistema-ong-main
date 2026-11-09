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

        // GET: Categoria/Details/5
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Categoria/Edit/5
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

        // POST: Categoria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    var vinculadas = await _context.Transacao.Where(t => t.Categoria == id).ToListAsync();
                    if (vinculadas.Any())
                    {
                        var outra = await _context.Categoria.AsNoTracking().FirstOrDefaultAsync(c => c.Id != id);
                        if (outra != null)
                        {
                            foreach (var t in vinculadas) t.Categoria = outra.Id;
                            _context.Transacao.UpdateRange(vinculadas);
                            await _context.SaveChangesAsync();
                        }
                    }
                    _context.Categoria.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Erro"] = "Não foi possível excluir esta categoria pois ela possui registros vinculados.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}

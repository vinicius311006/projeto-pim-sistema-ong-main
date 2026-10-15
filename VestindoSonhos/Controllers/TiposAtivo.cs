using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers
{
    public class TiposAtivo : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public TiposAtivo(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: TiposAtivo
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposAtivo.ToListAsync());
        }

        // GET: TiposAtivo/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposAtivo = await _context.TiposAtivo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tiposAtivo == null)
            {
                return NotFound();
            }

            return View(tiposAtivo);
        }

        // GET: TiposAtivo/Create
        [Authorize(Roles = "Administrador,Colaborador")]
        public IActionResult Criar()
        {
            return View();
        }

        // POST: TiposAtivo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Categoria")] Models.TiposAtivo tiposAtivo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tiposAtivo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tiposAtivo);
        }

        // GET: TiposAtivo/Editar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposAtivo = await _context.TiposAtivo.FindAsync(id);
            if (tiposAtivo == null)
            {
                return NotFound();
            }
            return View(tiposAtivo);
        }

        // POST: TiposAtivo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Categoria")] Models.TiposAtivo tiposAtivo)
        {
            if (id != tiposAtivo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tiposAtivo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TiposAtivoExists(tiposAtivo.Id))
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
            return View(tiposAtivo);
        }

        // GET: TiposAtivo/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tiposAtivo = await _context.TiposAtivo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tiposAtivo == null)
            {
                return NotFound();
            }

            return View(tiposAtivo);
        }

        // POST: TiposAtivo/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tiposAtivo = await _context.TiposAtivo.FindAsync(id);
                if (tiposAtivo != null)
                {
                    var produtos = await _context.Produto.Where(p => p.Categoria == id).ToListAsync();
                    foreach (var p in produtos)
                    {
                        p.Categoria = null;
                    }
                    if (produtos.Any())
                    {
                        await _context.SaveChangesAsync();
                    }

                    _context.TiposAtivo.Remove(tiposAtivo);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Falha ao excluir a categoria.");
                Console.WriteLine("Falha ao excluir a categoria.");
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool TiposAtivoExists(int id)
        {
            return _context.TiposAtivo.Any(e => e.Id == id);
        }
    }
}

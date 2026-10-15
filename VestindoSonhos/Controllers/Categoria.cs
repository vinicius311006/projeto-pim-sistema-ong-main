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
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoria.ToListAsync());
        }

        // GET: Categoria/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
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
        public IActionResult Criar(string returnUrl)
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
        public async Task<IActionResult> Criar([Bind("Id", "Nome")] Models.Categoria categoria, string returnUrl = null)
        {
            foreach (var state in ModelState)
            {
                Console.WriteLine($"Campo: {state.Key}");
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Erro: {error.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();

                return Redirect(returnUrl ?? "/Categoria/Index/");
            }

            return View(categoria);
        }

        // GET: Categoria/Edit/5
        [Authorize(Roles = "Administrador,Colaborador")]
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
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome")] Models.Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return NotFound();
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
        [Authorize(Roles = "Administrador,Colaborador")]
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
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);
                if (categoria != null)
                {
                    _context.Categoria.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                Console.WriteLine("Categoria vinculada a transações existentes.");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}

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
    public class Cargos : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Cargos(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Cargos
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cargo.ToListAsync());
        }

        // GET: Cargos/Details/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cargo = await _context.Cargo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cargo == null)
            {
                return NotFound();
            }

            return View(cargo);
        }

        // GET: Cargos/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View("Create");
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Criar()
        {
            return View("Create");
        }

        // POST: Cargos/Create
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Cargo cargo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cargo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Create", cargo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome")] Cargo cargo)
        {
            return await Create(cargo);
        }

        // GET: Cargos/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cargo = await _context.Cargo.FindAsync(id);
            if (cargo == null)
            {
                return NotFound();
            }
            return View("Edit", cargo);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int? id)
        {
            return await Edit(id);
        }

        // POST: Cargos/Edit/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Cargo cargo)
        {
            if (id != cargo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cargo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CargoExists(cargo.Id))
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
            return View("Edit", cargo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome")] Cargo cargo)
        {
            return await Edit(id, cargo);
        }

        // GET: Cargos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cargo = await _context.Cargo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cargo == null)
            {
                return NotFound();
            }

            return View("Delete", cargo);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            return await Delete(id);
        }

        // POST: Cargos/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cargo = await _context.Cargo.FindAsync(id);
            if (cargo != null)
            {
                _context.Cargo.Remove(cargo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            return await DeleteConfirmed(id);
        }

        private bool CargoExists(int id)
        {
            return _context.Cargo.Any(e => e.Id == id);
        }
    }
}
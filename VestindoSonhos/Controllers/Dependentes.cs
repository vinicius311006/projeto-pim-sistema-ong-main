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
    public class Dependentes : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Dependentes(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Dependentes
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            var vestindosonhosdbContext = _context.Dependente.Include(d => d.IdResponsavelNavigation);
            return View(await vestindosonhosdbContext.ToListAsync());
        }

        // GET: Dependentes/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dependente = await _context.Dependente
                .Include(d => d.IdResponsavelNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dependente == null)
            {
                return NotFound();
            }

            return View(dependente);
        }

        // GET: Dependentes/Criar
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Criar(int idBeneficiario)
        {
            var beneficiario = await _context.Beneficiario.FirstOrDefaultAsync(b => b.Id == idBeneficiario);
            ViewBag.Sexo = new SelectList(new[] { "M", "F", "n" });
            ViewBag.beneficiarioNome = beneficiario.Nome;
            ViewBag.beneficiarioId = idBeneficiario;
            return View();
        }

        // POST: Dependentes/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("IdResponsavel,Nome,Idade,Sexo,Parentesco")] Dependente model)
        {
            if (!string.IsNullOrEmpty(model.Nome) && model.Nome.Length > 60)
                model.Nome = model.Nome.Substring(0, 60);
            if (!string.IsNullOrEmpty(model.Parentesco) && model.Parentesco.Length > 20)
                model.Parentesco = model.Parentesco.Substring(0, 20);
            if (!string.IsNullOrEmpty(model.Sexo) && model.Sexo.Length > 1)
                model.Sexo = model.Sexo.Substring(0, 1);
            if (model.Idade < 0) model.Idade = 0;
            if (model.Idade > 120) model.Idade = 120;

            ModelState.Remove("IdResponsavelNavigation");
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Beneficiarios", new { id = model.IdResponsavel });
            }
            var resp = await _context.Beneficiario.FirstOrDefaultAsync(b => b.Id == model.IdResponsavel);
            ViewBag.Sexo = new SelectList(new[] { "M", "F", "n" }, model.Sexo);
            ViewBag.beneficiarioNome = resp?.Nome;
            ViewBag.beneficiarioId = model.IdResponsavel;
            return View(model);
        }

        // GET: Dependentes/Editar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var dependente = await _context.Dependente.FindAsync(id);
            if (dependente == null)
            {
                return NotFound();
            }
            var beneficiario = await _context.Beneficiario.FirstOrDefaultAsync(b => b.Id == dependente.IdResponsavel);

            ViewBag.beneficiarioNome = beneficiario?.Nome;
            ViewBag.beneficiarioId = beneficiario?.Id ?? dependente.IdResponsavel;
            ViewBag.Sexo = new SelectList(new[] { "M", "F", "n" }, dependente.Sexo);

            return View(dependente);
        }

        // POST: Dependentes/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id, IdResponsavel,Nome,Idade,Sexo,Parentesco")] Dependente dependente)
        {
            if (id != dependente.Id)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(dependente.Nome) && dependente.Nome.Length > 60)
                dependente.Nome = dependente.Nome.Substring(0, 60);
            if (!string.IsNullOrEmpty(dependente.Parentesco) && dependente.Parentesco.Length > 20)
                dependente.Parentesco = dependente.Parentesco.Substring(0, 20);
            if (!string.IsNullOrEmpty(dependente.Sexo) && dependente.Sexo.Length > 1)
                dependente.Sexo = dependente.Sexo.Substring(0, 1);
            if (dependente.Idade < 0) dependente.Idade = 0;
            if (dependente.Idade > 120) dependente.Idade = 120;

            ModelState.Remove("IdResponsavelNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dependente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DependenteExists(dependente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Beneficiarios", new { id = dependente.IdResponsavel });
            }
            var resp = await _context.Beneficiario.FirstOrDefaultAsync(b => b.Id == dependente.IdResponsavel);
            ViewBag.Sexo = new SelectList(new[] { "M", "F", "n" }, dependente.Sexo);
            ViewBag.beneficiarioNome = resp?.Nome;
            ViewBag.beneficiarioId = dependente.IdResponsavel;
            return View(dependente);
        }

        // GET: Dependentes/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dependente = await _context.Dependente
                .Include(d => d.IdResponsavelNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dependente == null)
            {
                return NotFound();
            }

            return View(dependente);
        }

        // POST: Dependentes/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dependente = await _context.Dependente.FindAsync(id);
            if (dependente != null)
            {
                _context.Dependente.Remove(dependente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(controllerName: "Beneficiarios", actionName: "Index");
        }

        private bool DependenteExists(int id)
        {
            return _context.Dependente.Any(e => e.Id == id);
        }
    }
}

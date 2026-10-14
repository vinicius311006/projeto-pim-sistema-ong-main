using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers
{
    public class Voluntarios : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Voluntarios(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Voluntarios
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Voluntario.ToListAsync());
        }

        // GET: Voluntarios/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voluntario = await _context.Voluntario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voluntario == null)
            {
                return NotFound();
            }

            return View(voluntario);
        }

        // GET: Voluntarios/Criar
        [Authorize(Roles = "Administrador,Colaborador")]
        public IActionResult Criar()
        {
            return View();
        }

        // POST: Voluntarios/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome,Cpf,Endereco,Telefone,Email")] Voluntario voluntario)
        {
            SanitizarVoluntario(voluntario);

            if (ModelState.IsValid)
            {
                _context.Add(voluntario);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index", controllerName: "Eventos");
            }
            return View(voluntario);
        }

        // GET: Voluntarios/Editar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voluntario = await _context.Voluntario.FindAsync(id);
            if (voluntario == null)
            {
                return NotFound();
            }
            return View(voluntario);
        }

        // POST: Voluntarios/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Cpf,Endereco,Telefone,Email")] Voluntario voluntario)
        {
            if (id != voluntario.Id)
            {
                return NotFound();
            }

            SanitizarVoluntario(voluntario);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voluntario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoluntarioExists(voluntario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(actionName: "Index", controllerName: "Eventos");
            }
            return View(voluntario);
        }

        // GET: Voluntarios/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voluntario = await _context.Voluntario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voluntario == null)
            {
                return NotFound();
            }

            return View(voluntario);
        }

        // POST: Voluntarios/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            var voluntario = await _context.Voluntario.FindAsync(id);
            if (voluntario != null)
            {
                _context.Voluntario.Remove(voluntario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index", controllerName: "Eventos");
        }

        private void SanitizarVoluntario(Voluntario voluntario)
        {
            if (voluntario == null) return;

            if (!string.IsNullOrEmpty(voluntario.Telefone))
            {
                voluntario.Telefone = Regex.Replace(voluntario.Telefone, @"\D", "");
                if (voluntario.Telefone.Length > 11)
                    voluntario.Telefone = voluntario.Telefone.Substring(0, 11);
            }

            if (!string.IsNullOrEmpty(voluntario.Cpf))
            {
                voluntario.Cpf = Regex.Replace(voluntario.Cpf, @"\D", "");
                if (voluntario.Cpf.Length > 11)
                    voluntario.Cpf = voluntario.Cpf.Substring(0, 11);
            }

            if (!string.IsNullOrEmpty(voluntario.Nome) && voluntario.Nome.Length > 60)
            {
                voluntario.Nome = voluntario.Nome.Substring(0, 60);
            }

            if (!string.IsNullOrEmpty(voluntario.Endereco) && voluntario.Endereco.Length > 50)
            {
                voluntario.Endereco = voluntario.Endereco.Substring(0, 50);
            }

            if (!string.IsNullOrEmpty(voluntario.Email) && voluntario.Email.Length > 40)
            {
                voluntario.Email = voluntario.Email.Substring(0, 40);
            }
        }

        private bool VoluntarioExists(int id)
        {
            return _context.Voluntario.Any(e => e.Id == id);
        }
    }
}

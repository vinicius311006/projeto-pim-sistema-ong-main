using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VestindoSonhos.Helpers;
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

        // GET: Voluntarios/Detalhes/5
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
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Voluntarios/Editarar/5
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

        // POST: Voluntarios/Editarar/5
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
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
                var atribuicoes = await _context.VoluntarioAtribuido
                    .Where(va => va.Voluntario == id)
                    .ToListAsync();
                if (atribuicoes.Any())
                {
                    _context.VoluntarioAtribuido.RemoveRange(atribuicoes);
                    await _context.SaveChangesAsync();
                }

                _context.Voluntario.Remove(voluntario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(actionName: "Index", controllerName: "Eventos");
        }

        private void SanitizarVoluntario(Voluntario voluntario)
        {
            if (voluntario == null) return;

            voluntario.Nome = voluntario.Nome?.Trim() ?? string.Empty;
            voluntario.Endereco = voluntario.Endereco?.Trim();
            voluntario.Email = voluntario.Email?.Trim();

            if (string.IsNullOrWhiteSpace(voluntario.Nome))
            {
                ModelState.Remove(nameof(voluntario.Nome));
                ModelState.AddModelError(nameof(voluntario.Nome), "O nome do voluntário é obrigatório.");
            }
            else if (voluntario.Nome.Length > 60)
            {
                voluntario.Nome = voluntario.Nome.Substring(0, 60);
            }

            if (!string.IsNullOrWhiteSpace(voluntario.Telefone))
            {
                voluntario.Telefone = Regex.Replace(voluntario.Telefone, @"\D", "");
                if (voluntario.Telefone.Length > 11)
                    voluntario.Telefone = voluntario.Telefone.Substring(0, 11);

                if (!ValidadorUsuarioHelper.TelefoneEhValido(voluntario.Telefone))
                {
                    ModelState.Remove(nameof(voluntario.Telefone));
                    ModelState.AddModelError(nameof(voluntario.Telefone), "Informe um telefone válido completo com DDD (10 ou 11 dígitos).");
                }
            }
            else
            {
                voluntario.Telefone = null;
            }

            voluntario.Cpf = Regex.Replace(voluntario.Cpf ?? string.Empty, @"\D", "");
            if (voluntario.Cpf.Length > 11)
                voluntario.Cpf = voluntario.Cpf.Substring(0, 11);

            if (string.IsNullOrWhiteSpace(voluntario.Cpf))
            {
                ModelState.Remove(nameof(voluntario.Cpf));
                ModelState.AddModelError(nameof(voluntario.Cpf), "O CPF do voluntário é obrigatório.");
            }
            else if (!ValidadorUsuarioHelper.CpfEhValido(voluntario.Cpf))
            {
                ModelState.Remove(nameof(voluntario.Cpf));
                ModelState.AddModelError(nameof(voluntario.Cpf), "CPF inválido. Verifique os 11 dígitos informados.");
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

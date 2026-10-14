using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Beneficiarios : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Beneficiarios(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Beneficiarios
        [Authorize(Roles = "Colaborador,Administrador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Beneficiario.ToListAsync());
        }

        // GET: Beneficiarios/Details/5
        [Authorize(Roles = "Colaborador,Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var contexto = new DetalhesBeneficiarioViewModel
            {
                listaBeneficiario = await _context.Beneficiario
                .ToListAsync(),
                beneficiario = await _context.Beneficiario
                .FirstOrDefaultAsync(m => m.Id == id),
                listaDependentes = await _context.Dependente
                .Where(d => d.IdResponsavel == id)
                .ToListAsync()
            };
            
            if (contexto.beneficiario == null)
            {
                return NotFound();
            }

            return View(contexto);
        }

        public void CarregarDropdowns()
        {
            ViewBag.Situacao = new SelectList(new[] { "Ok", "Aguardando Doacao", "Bloqueado" });
        }

        // GET: Beneficiarios/Criar
        [Authorize(Roles = "Colaborador,Administrador")]
        public IActionResult Criar()
        {
            CarregarDropdowns();
            return View();
        }

        // POST: Beneficiarios/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Colaborador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome,Telefone,Endereco,Situacao,Observacao")] Beneficiario beneficiario)
        {
            if (!string.IsNullOrEmpty(beneficiario.Telefone))
            {
                beneficiario.Telefone = System.Text.RegularExpressions.Regex.Replace(beneficiario.Telefone, @"\D", "");
                if (beneficiario.Telefone.Length > 11)
                {
                    beneficiario.Telefone = beneficiario.Telefone.Substring(0, 11);
                }
            }

            if (!string.IsNullOrEmpty(beneficiario.Nome) && beneficiario.Nome.Length > 80)
            {
                beneficiario.Nome = beneficiario.Nome.Substring(0, 80);
            }
            if (!string.IsNullOrEmpty(beneficiario.Endereco) && beneficiario.Endereco.Length > 60)
            {
                beneficiario.Endereco = beneficiario.Endereco.Substring(0, 60);
            }
            if (!string.IsNullOrEmpty(beneficiario.Observacao) && beneficiario.Observacao.Length > 255)
            {
                beneficiario.Observacao = beneficiario.Observacao.Substring(0, 255);
            }

            List<string> situacoes = new List<string> { "Ok", "Aguardando Doacao", "Bloqueado" };
            if (!string.IsNullOrEmpty(beneficiario.Situacao) && !situacoes.Contains(beneficiario.Situacao))
            {
                ModelState.AddModelError(nameof(beneficiario.Situacao), "Situação inválida.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(beneficiario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CarregarDropdowns();
            return View(beneficiario);
        }

        // GET: Beneficiarios/Editar/5
        [Authorize(Roles = "Colaborador,Administrador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beneficiario = await _context.Beneficiario.FindAsync(id);
            if (beneficiario == null)
            {
                return NotFound();
            }
            CarregarDropdowns();
            return View(beneficiario);
        }

        // POST: Beneficiarios/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Colaborador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Telefone,Endereco,Situacao,Observacao")] Beneficiario beneficiario)
        {
            if (id != beneficiario.Id)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(beneficiario.Telefone))
            {
                beneficiario.Telefone = System.Text.RegularExpressions.Regex.Replace(beneficiario.Telefone, @"\D", "");
                if (beneficiario.Telefone.Length > 11)
                {
                    beneficiario.Telefone = beneficiario.Telefone.Substring(0, 11);
                }
            }

            if (!string.IsNullOrEmpty(beneficiario.Nome) && beneficiario.Nome.Length > 80)
            {
                beneficiario.Nome = beneficiario.Nome.Substring(0, 80);
            }
            if (!string.IsNullOrEmpty(beneficiario.Endereco) && beneficiario.Endereco.Length > 60)
            {
                beneficiario.Endereco = beneficiario.Endereco.Substring(0, 60);
            }
            if (!string.IsNullOrEmpty(beneficiario.Observacao) && beneficiario.Observacao.Length > 255)
            {
                beneficiario.Observacao = beneficiario.Observacao.Substring(0, 255);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(beneficiario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeneficiarioExists(beneficiario.Id))
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
            CarregarDropdowns();
            return View(beneficiario);
        }

        // GET: Beneficiarios/Apagar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beneficiario = await _context.Beneficiario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (beneficiario == null)
            {
                return NotFound();
            }

            return View(beneficiario);
        }

        // POST: Beneficiarios/Apagar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beneficiario = await _context.Beneficiario.FindAsync(id);
            if (beneficiario != null)
            {
                _context.Beneficiario.Remove(beneficiario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Colaborador,Administrador")]
        private bool BeneficiarioExists(int id)
        {
            return _context.Beneficiario.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Helpers;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers
{
    public class Carteiras : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Carteiras(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Carteiras
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categorias = await _context.Categoria.OrderBy(c => c.Nome).ToListAsync();
            return View(await _context.Carteira.ToListAsync());
        }

        // GET: Carteiras/Detalhes/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carteira = await _context.Carteira
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carteira == null)
            {
                return NotFound();
            }

            return View(carteira);
        }

        // GET: Carteiras/Criar
        [Authorize(Roles = "Administrador")]
        public IActionResult Criar()
        {
            return View();
        }

        // POST: Carteiras/Criar
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome,Saldo")] Carteira carteira)
        {
            ModelState.Remove(nameof(Carteira.Saldo));

            carteira.Nome = carteira.Nome?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(carteira.Nome))
            {
                ModelState.AddModelError(nameof(carteira.Nome), "O nome da carteira é obrigatório.");
            }
            else if (carteira.Nome.Length > 60)
            {
                carteira.Nome = carteira.Nome.Substring(0, 60);
            }

            decimal saldoInformado = carteira.Saldo;
            if (Request?.HasFormContentType == true && Request.Form.ContainsKey("Saldo"))
            {
                var saldoTexto = Request.Form["Saldo"].ToString();
                if (!string.IsNullOrWhiteSpace(saldoTexto) && !saldoTexto.Any(char.IsDigit))
                {
                    ModelState.AddModelError(nameof(carteira.Saldo), "Informe um valor numérico válido para o saldo.");
                }
                else
                {
                    saldoInformado = MoedaHelper.ConverterValorMonetario(saldoTexto);
                }
            }

            if (saldoInformado < -99_999_999.99m || saldoInformado > 99_999_999.99m)
            {
                ModelState.AddModelError(nameof(carteira.Saldo), "O saldo deve estar entre -R$ 99.999.999,99 e R$ 99.999.999,99.");
            }

            carteira.Saldo = saldoInformado;

            if (ModelState.IsValid)
            {
                _context.Add(carteira);
                await _context.SaveChangesAsync();
                if (TempData != null)
                {
                    TempData["Sucesso"] = "Carteira criada com sucesso!";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(carteira);
        }

        // GET: Carteiras/Editar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int? id, string? returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carteira = await _context.Carteira.FindAsync(id);
            if (carteira == null)
            {
                return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(carteira);
        }

        // POST: Carteiras/Editar/5
        // Para proteger contra ataques de overposting, habilite apenas as propriedades específicas que deseja vincular.
        // Para mais detalhes, consulte http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Saldo")] Carteira carteira, string? returnUrl = null)
        {
            if (id != carteira.Id)
            {
                return NotFound();
            }

            var carteiraDb = await _context.Carteira.FindAsync(id);
            if (carteiraDb == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Carteira.Saldo));

            carteira.Nome = carteira.Nome?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(carteira.Nome))
            {
                ModelState.AddModelError(nameof(carteira.Nome), "O nome da carteira é obrigatório.");
            }
            else if (carteira.Nome.Length > 60)
            {
                carteira.Nome = carteira.Nome.Substring(0, 60);
            }

            decimal saldoInformado = carteira.Saldo;
            if (Request?.HasFormContentType == true && Request.Form.ContainsKey("Saldo"))
            {
                var saldoTexto = Request.Form["Saldo"].ToString();
                if (string.IsNullOrWhiteSpace(saldoTexto) || !saldoTexto.Any(char.IsDigit))
                {
                    ModelState.AddModelError(nameof(carteira.Saldo), "Informe um valor numérico válido para o saldo (ex: 5.000,00 ou 0,00).");
                }
                else
                {
                    saldoInformado = MoedaHelper.ConverterValorMonetario(saldoTexto);
                }
            }

            if (saldoInformado < -99_999_999.99m || saldoInformado > 99_999_999.99m)
            {
                ModelState.AddModelError(nameof(carteira.Saldo), "O saldo deve estar entre -R$ 99.999.999,99 e R$ 99.999.999,99.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    carteiraDb.Nome = carteira.Nome;
                    carteiraDb.Saldo = saldoInformado;
                    _context.Update(carteiraDb);
                    await _context.SaveChangesAsync();
                    if (TempData != null)
                    {
                        TempData["Sucesso"] = "Carteira e saldo atualizados com sucesso!";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarteiraExists(carteira.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
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

            ViewBag.ReturnUrl = returnUrl;
            carteira.Saldo = saldoInformado;
            return View(carteira);
        }

        // GET: Carteiras/Apagar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carteira = await _context.Carteira
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carteira == null)
            {
                return NotFound();
            }

            return View(carteira);
        }

        // POST: Carteiras/Apagar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carteira = await _context.Carteira.FindAsync(id);
            if (carteira != null)
            {
                if(carteira.Saldo != 0)
                {
                    return RedirectToAction(controllerName: "Erro", actionName: "ContaNaoVazia");
                }
                var transacoesFin = await _context.TransacaoFinanceira.Where(t => t.Carteira == id).ToListAsync();
                if (transacoesFin.Any())
                {
                    _context.TransacaoFinanceira.RemoveRange(transacoesFin);
                }
                _context.Carteira.Remove(carteira);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarteiraExists(int id)
        {
            return _context.Carteira.Any(e => e.Id == id);
        }
    }
}

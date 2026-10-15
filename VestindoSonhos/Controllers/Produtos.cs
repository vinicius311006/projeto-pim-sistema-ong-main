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
    public class Produtos : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Produtos(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Produtos
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            var vestindosonhosdbContext = _context.Produto.Include(p => p.CategoriaNavigation);
            return View(await vestindosonhosdbContext.ToListAsync());
        }

        // GET: Produtos/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto.Include(p => p.CategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        private void CarregarDropdowns()
        {
            ViewData["Categoria"] = new SelectList(_context.TiposAtivo, "Id", "Categoria");
        }

        // GET: Produtos/Criar
        [Authorize(Roles = "Colaborador,Administrador")]
        public IActionResult Criar()
        {
            CarregarDropdowns();
            return View();
        }

        // POST: Produtos/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Colaborador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Nome,Descricao,Cor,Categoria")] Produto produto)
        {
            if (!string.IsNullOrEmpty(produto.Nome) && produto.Nome.Length > 60)
                produto.Nome = produto.Nome.Substring(0, 60);
            if (!string.IsNullOrEmpty(produto.Descricao) && produto.Descricao.Length > 255)
                produto.Descricao = produto.Descricao.Substring(0, 255);
            if (!string.IsNullOrEmpty(produto.Cor) && produto.Cor.Length > 15)
                produto.Cor = produto.Cor.Substring(0, 15);

            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CarregarDropdowns();
            return View(produto);
        }

        // GET: Produtos/Editar/5
        [Authorize(Roles = "Colaborador,Administrador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            CarregarDropdowns();
            return View(produto);
        }

        // POST: Produtos/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Colaborador,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Descricao,Cor,Categoria")] Produto produto)
        {
            if (id != produto.Id)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(produto.Nome) && produto.Nome.Length > 60)
                produto.Nome = produto.Nome.Substring(0, 60);
            if (!string.IsNullOrEmpty(produto.Descricao) && produto.Descricao.Length > 255)
                produto.Descricao = produto.Descricao.Substring(0, 255);
            if (!string.IsNullOrEmpty(produto.Cor) && produto.Cor.Length > 15)
                produto.Cor = produto.Cor.Substring(0, 15);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
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
            return View(produto);
        }

        // GET: Produtos/Apagar/5
        [Authorize(Roles = "Colaborador,Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto
                .Include(p => p.CategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        // POST: Produtos/Apagar/5
        [Authorize(Roles = "Colaborador,Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            var produto = await _context.Produto.FindAsync(id);
            if (produto != null)
            {
                var transacoesInv = await _context.TransacaoInventario
                    .Where(ti => ti.ProdutoId == id)
                    .ToListAsync();
                if (transacoesInv.Any())
                {
                    _context.TransacaoInventario.RemoveRange(transacoesInv);
                    await _context.SaveChangesAsync();
                }

                _context.Produto.Remove(produto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produto.Any(e => e.Id == id);
        }
    }
}

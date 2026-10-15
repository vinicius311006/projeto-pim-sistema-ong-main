using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Inventario : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Inventario(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Inventario
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            var transacoes_simples = await (from ti in _context.TransacaoInventario
                                      join p in _context.Produto on ti.ProdutoId equals p.Id
                                      join t in _context.Transacao on ti.TransacaoId equals t.Id
                                      orderby t.Hora descending
                                      select new InventarioSimplesViewModel
                                      {
                                          TransacaoId = t.Id,
                                          Id = ti.ProdutoId,
                                          NomeProduto = p.Nome,
                                          Quantidade = ti.Quantidade,
                                          Razao = t.Razao,
                                          Hora = t.Hora
                                      }).ToListAsync();
            return View(transacoes_simples);
        }

        // GET: Inventario/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao_inventario = await _context.TransacaoInventario.Include(ti => ti.BeneficiarioNavigation).Include(ti => ti.ProdutoNavigation).FirstOrDefaultAsync(ti => ti.TransacaoId == id);
            var transacao = await _context.Transacao.Include(t => t.CategoriaNavigation).Include(t => t.UsuarioNavigation).FirstOrDefaultAsync(t => t.Id == id);
            var itens = await (from ti in _context.TransacaoInventario
                               join p in _context.Produto on ti.ProdutoId equals p.Id
                               where ti.TransacaoId == id
                               select new TransacaoInventarioViewModel.ItemTransacao
                               {
                                   ProdutoId = ti.ProdutoId,
                                   Quantidade = ti.Quantidade,
                                   ProdutoNavigation = ti.ProdutoNavigation,
                                   Categoria = p.Categoria,
                                   CategoriaNavigation = p.CategoriaNavigation
                               }).ToListAsync();
            var contexto = new TransacaoInventarioViewModel
            {
                IdTransacao = transacao.Id,
                Usuario = transacao.Usuario,
                UsuarioNavigation = transacao.UsuarioNavigation,
                Categoria = transacao.Categoria,
                CategoriaNavigation = transacao.CategoriaNavigation,
                Hora = transacao.Hora,
                Observacao = transacao.Observacao,
                Razao = transacao.Razao.ToString(),
                Beneficiario = transacao_inventario.Beneficiario,
                BeneficiarioNavigation = transacao_inventario.BeneficiarioNavigation,
                listaItensAtribuidos = itens
            };

            if (contexto == null)
            {
                return NotFound();
            }

            return View(contexto);
        }

        public void CarregarDropdowns()
        {

            ViewBag.Razao = new SelectList(new[] { "Entrada", "Saída" });
            ViewData["Categoria"] = new SelectList(_context.Categoria, "Id", "Nome");
            ViewData["Beneficiarios"] = new SelectList(_context.Beneficiario, "Id", "Nome");
        }

        // GET: Inventario/Criar
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Criar()
        {
            CarregarDropdowns();
            var produtos = new TransacaoInventarioCriacaoEdicaoViewModel
            {
                Produtos = await (from p in _context.Produto
                            join ta in _context.TiposAtivo on p.Categoria equals ta.Id into taGroup
                            from ta in taGroup.DefaultIfEmpty()
                            select new TransacaoInventarioCriacaoEdicaoViewModel.ItemTransacaoCriacaoEdicao
                            {
                                ProdutoId = p.Id,
                                CategoriaProduto = p.Categoria,
                                NomeCategoriaProduto = ta != null ? ta.Categoria : "Geral",
                                NomeProduto = p.Nome,
                                Cor = p.Cor,
                                Descricao = p.Descricao,
                                Selecionado = false
                            }).ToListAsync()
            };
            return View(produtos);
        }

        //// GET: Inventario/Criar
        //[Authorize(Roles = "Administrador")]
        //public async Task<IActionResult> Criar(List<TransacaoInventarioCriacaoEdicaoViewModel.ItemTransacaoCriacaoEdicao> itensSelecionados)
        //{
        //    CarregarDropdowns();
        //    var produtos = new TransacaoInventarioCriacaoEdicaoViewModel
        //    {
        //        Produtos = await (from p in _context.Produto
        //                          join ta in _context.TiposAtivo on p.Categoria equals ta.Id
        //                          select new TransacaoInventarioCriacaoEdicaoViewModel.ItemTransacaoCriacaoEdicao
        //                          {
        //                              ProdutoId = p.Id,
        //                              CategoriaProduto = p.Categoria,
        //                              NomeCategoriaProduto = ta.Categoria,
        //                              NomeProduto = p.Nome,
        //                              Cor = p.Cor,
        //                              Descricao = p.Descricao,
        //                              Selecionado = false
        //                          }).ToListAsync()
        //    };
        //    foreach(var item in produtos.Produtos)
        //    {
        //        if(item.ProdutoId in itensSelecionados){

        //        }
        //    }
        //    return View(produtos);
        //}

        // POST: Inventario/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Produtos,CategoriaTransacao,Beneficiario,Observacao,Razao")] TransacaoInventarioCriacaoEdicaoViewModel model)
        {
            CarregarDropdowns();
            var itensSelecionados = model.Produtos?.Where(p => p.Selecionado && p.Quantidade > 0).ToList();
            if (itensSelecionados == null || !itensSelecionados.Any())
            {
                ModelState.AddModelError("", "Por favor, selecione ao menos um produto na tabela e informe a quantidade.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Observacao) && model.Observacao.Length > 255)
            {
                model.Observacao = model.Observacao.Substring(0, 255);
            }

            if (ModelState.IsValid)
            {
                var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int idUsuario = 1;
                if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
                {
                    idUsuario = parsedId;
                }

                var transacao = new Transacao
                {
                    Usuario = idUsuario,
                    Hora = DateTime.Now,
                    Categoria = model.CategoriaTransacao,
                    Observacao = model.Observacao,
                    Razao = model.Razao
                };
                try
                {
                    _context.Transacao.Add(transacao);
                    await _context.SaveChangesAsync();
                    foreach (var produto in itensSelecionados)
                    {
                        var transacao_inventario = new TransacaoInventario
                        {
                            Beneficiario = model.Beneficiario,
                            TransacaoId = transacao.Id,
                            ProdutoId = produto.ProdutoId,
                            Quantidade = produto.Quantidade
                        };
                        _context.TransacaoInventario.Add(transacao_inventario);
                    }
                    await _context.SaveChangesAsync();                    
                }
                catch
                {
                    Console.WriteLine("Falha ao salvar transação.");
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Inventario/Editar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Editar(int id)
        {
            CarregarDropdowns();
            var transacao = await _context.Transacao.Include(t => t.CategoriaNavigation).FirstOrDefaultAsync(t => t.Id == id);
            var transacaoInventario = await _context.TransacaoInventario.Include(ti => ti.BeneficiarioNavigation).Where(ti => ti.TransacaoId == id).ToListAsync();

            if (transacao == null || !transacaoInventario.Any())
            {
                return RedirectToAction(controllerName: "Erro", actionName: "InventarioInvalida");
            }
            Console.WriteLine(transacao.ToString());
            Console.WriteLine(transacaoInventario);

            var primeiroItem = transacaoInventario.First();

            var contexto = new TransacaoInventarioCriacaoEdicaoViewModel
            {
                IdTransacao = id,
                Beneficiario = primeiroItem.Beneficiario,
                NomeBeneficiario = primeiroItem.BeneficiarioNavigation?.Nome,
                CategoriaTransacao = transacao.Categoria,
                Razao = transacao.Razao,
                Observacao = transacao.Observacao,
                NomeCategoriaTransacao = transacao.CategoriaNavigation.Nome,
                Produtos = await (from p in _context.Produto
                                  join ta in _context.TiposAtivo on p.Categoria equals ta.Id into taGroup
                                  from ta in taGroup.DefaultIfEmpty()
                                  select new TransacaoInventarioCriacaoEdicaoViewModel.ItemTransacaoCriacaoEdicao
                                  {
                                      ProdutoId = p.Id,
                                      CategoriaProduto = p.Categoria,
                                      NomeCategoriaProduto = ta != null ? ta.Categoria : "Geral",
                                      NomeProduto = p.Nome,
                                      Cor = p.Cor,
                                      Descricao = p.Descricao,
                                      Selecionado = false,
                                      Quantidade = 0
                                  }).ToListAsync()
            };
            foreach(var transacaoInv in transacaoInventario)
            {
                var produto_preencher = contexto.Produtos.FirstOrDefault(p => p.ProdutoId == transacaoInv.ProdutoId);
                if (produto_preencher != null)
                {
                    produto_preencher.Quantidade = transacaoInv.Quantidade;

                    produto_preencher.Selecionado = true;
                }
            }
            if (contexto == null)
            {
                return RedirectToAction(controllerName: "Erro", actionName: "InventarioInvalida");
            }
            return View(contexto);
        }

        // POST: Inventario/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("IdTransacao,Produtos,CategoriaTransacao,Beneficiario,Observacao,Razao")] TransacaoInventarioCriacaoEdicaoViewModel model)
        {
            if (id != model.IdTransacao)
            {
                Console.WriteLine(id);
                Console.WriteLine(model.IdTransacao);
                return NotFound();
            }
            foreach (var p in model.Produtos)
            {
                Console.WriteLine(
                    $"Produto {p.ProdutoId} | " +
                    $"Selecionado: {p.Selecionado} | " +
                    $"Quantidade: {p.Quantidade}");
            }

            if (ModelState.IsValid)
            {

                var transacao = await _context.Transacao.FirstOrDefaultAsync(t => t.Id == id);
                if (transacao == null)
                {
                    return NotFound();
                }
                transacao.Categoria = model.CategoriaTransacao;
                transacao.Razao = model.Razao;
                if (!string.IsNullOrWhiteSpace(model.Observacao))
                {
                    transacao.Observacao = model.Observacao;
                }
                transacao.Hora = DateTime.Now;
                try
                {
                    _context.Transacao.Update(transacao);
                    await _context.TransacaoInventario.Where(ti => ti.TransacaoId == id).ExecuteDeleteAsync();
                    var produtosSelecionados = model.Produtos.Where(p => p.Selecionado == true && p.Quantidade > 0);
                    foreach (var produtoAdicionado in produtosSelecionados)
                    {
                        var transacaoInventario = new TransacaoInventario
                        {
                            TransacaoId = id,
                            Beneficiario = model.Beneficiario,
                            ProdutoId = produtoAdicionado.ProdutoId,
                            Quantidade = produtoAdicionado.Quantidade
                        };
                        await _context.TransacaoInventario.AddAsync(transacaoInventario);
                    }
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    Console.WriteLine("Erro ao atualizar o banco");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Inventario/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao_inventario = await _context.TransacaoInventario.Include(ti => ti.BeneficiarioNavigation).Include(ti => ti.ProdutoNavigation).FirstOrDefaultAsync(ti => ti.TransacaoId == id);
            var transacao = await _context.Transacao.Include(t => t.CategoriaNavigation).Include(t => t.UsuarioNavigation).FirstOrDefaultAsync(t => t.Id == id);
            if (transacao == null)
            {
                return NotFound();
            }

            var itens = await (from ti in _context.TransacaoInventario
                               join p in _context.Produto on ti.ProdutoId equals p.Id
                               where ti.TransacaoId == id
                               select new TransacaoInventarioViewModel.ItemTransacao
                               {
                                   ProdutoId = ti.ProdutoId,
                                   Quantidade = ti.Quantidade,
                                   ProdutoNavigation = ti.ProdutoNavigation,
                                   Categoria = p.Categoria,
                                   CategoriaNavigation = p.CategoriaNavigation
                               }).ToListAsync();
            var contexto = new TransacaoInventarioViewModel
            {
                IdTransacao = transacao.Id,
                Usuario = transacao.Usuario,
                UsuarioNavigation = transacao.UsuarioNavigation,
                Categoria = transacao.Categoria,
                CategoriaNavigation = transacao.CategoriaNavigation,
                Hora = transacao.Hora,
                Observacao = transacao.Observacao,
                Razao = transacao.Razao.ToString(),
                Beneficiario = transacao_inventario?.Beneficiario,
                BeneficiarioNavigation = transacao_inventario?.BeneficiarioNavigation,
                listaItensAtribuidos = itens
            };

            return View(contexto);
        }

        // POST: Inventario/Apagar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConfirmed(int id)
        {
            var transacoesInventario = await _context.TransacaoInventario.Where(ti => ti.TransacaoId == id).ToListAsync();
            if (transacoesInventario.Any())
            {
                _context.TransacaoInventario.RemoveRange(transacoesInventario);
                await _context.SaveChangesAsync();
            }

            var transacao = await _context.Transacao.FirstOrDefaultAsync(t => t.Id == id);
            if (transacao != null)
            {
                _context.Transacao.Remove(transacao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TransacaoInventarioExists(int id)
        {
            return _context.TransacaoInventario.Any(e => e.ProdutoId == id);
        }
    }


}

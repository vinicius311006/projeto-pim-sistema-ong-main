using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    [ApiController]
    [Route("api/inventario")]
    [Authorize]
    public class InventarioApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;

        public InventarioApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventarioItemDto>>> ObterInventario()
        {
            var produtos = await _context.Produto
                .AsNoTracking()
                .Include(p => p.CategoriaNavigation)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            var movimentacoes = await (from ti in _context.TransacaoInventario
                                       join t in _context.Transacao on ti.TransacaoId equals t.Id
                                       select new
                                       {
                                           ti.ProdutoId,
                                           ti.Quantidade,
                                           t.Razao
                                       })
                                       .AsNoTracking()
                                       .ToListAsync();

            var lista = produtos.Select(p =>
            {
                var entradas = movimentacoes
                    .Where(m => m.ProdutoId == p.Id && m.Razao == "Entrada")
                    .Sum(m => m.Quantidade);

                var saidas = movimentacoes
                    .Where(m => m.ProdutoId == p.Id && (m.Razao == "Saida" || m.Razao == "Saída"))
                    .Sum(m => m.Quantidade);

                var estoqueAtual = Math.Max(0, entradas - saidas);

                return new InventarioItemDto
                {
                    ProdutoId = p.Id,
                    NomeProduto = p.Nome,
                    Descricao = p.Descricao,
                    Cor = p.Cor,
                    Categoria = p.CategoriaNavigation != null ? p.CategoriaNavigation.Categoria : "Geral",
                    QuantidadeEmEstoque = estoqueAtual
                };
            }).ToList();

            return Ok(lista);
        }
    }
}

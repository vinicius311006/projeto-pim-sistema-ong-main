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

        [HttpPost("movimentacao")]
        public async Task<IActionResult> RegistrarMovimentacao([FromBody] CriarMovimentacaoDto dto)
        {
            if (dto.ProdutoId <= 0)
            {
                return BadRequest(new { mensagem = "Selecione um produto válido." });
            }

            if (dto.Quantidade <= 0)
            {
                return BadRequest(new { mensagem = "A quantidade deve ser maior que zero." });
            }

            var produto = await _context.Produto.FindAsync(dto.ProdutoId);
            if (produto == null)
            {
                return NotFound(new { mensagem = "Produto não encontrado." });
            }

            var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int idUsuario = 1;
            if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
            {
                idUsuario = parsedId;
            }

            var categoriaPadrao = await _context.Categoria.FirstOrDefaultAsync();
            int catId = categoriaPadrao?.Id ?? 1;

            var obs = dto.Observacao?.Trim();
            if (obs != null && obs.Length > 255) obs = obs.Substring(0, 255);

            var transacao = new Transacao
            {
                Usuario = idUsuario,
                Hora = DateTime.Now,
                Categoria = catId,
                Observacao = obs,
                Razao = dto.Razao == "Saída" || dto.Razao == "Saida" ? "Saída" : "Entrada"
            };

            _context.Transacao.Add(transacao);
            await _context.SaveChangesAsync();

            var transacaoInventario = new TransacaoInventario
            {
                TransacaoId = transacao.Id,
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                Beneficiario = dto.BeneficiarioId
            };

            _context.TransacaoInventario.Add(transacaoInventario);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Movimentação registrada com sucesso!" });
        }

        [HttpPost("produtos")]
        public async Task<ActionResult<InventarioItemDto>> CriarProduto([FromBody] CriarProdutoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do produto é obrigatório." });
            }

            var nome = dto.Nome.Trim();
            if (nome.Length > 60) nome = nome.Substring(0, 60);

            var descricao = (dto.Descricao ?? "").Trim();
            if (descricao.Length > 255) descricao = descricao.Substring(0, 255);

            var cor = (dto.Cor ?? "").Trim();
            if (cor.Length > 15) cor = cor.Substring(0, 15);

            // Categoria (TiposAtivo)
            int? catId = dto.CategoriaId;
            if (catId == null)
            {
                var tipoAtivo = await _context.TiposAtivo.FirstOrDefaultAsync();
                catId = tipoAtivo?.Id;
            }

            var produto = new Produto
            {
                Nome = nome,
                Descricao = descricao,
                Cor = cor,
                Categoria = catId
            };

            _context.Produto.Add(produto);
            await _context.SaveChangesAsync();

            var catNome = "Geral";
            if (produto.Categoria != null)
            {
                var cat = await _context.TiposAtivo.FindAsync(produto.Categoria);
                if (cat != null) catNome = cat.Categoria;
            }

            return Ok(new InventarioItemDto
            {
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                Descricao = produto.Descricao,
                Cor = produto.Cor,
                Categoria = catNome,
                QuantidadeEmEstoque = 0
            });
        }

        [HttpDelete("produtos/{id:int}")]
        public async Task<IActionResult> ExcluirProduto(int id)
        {
            var p = await _context.Produto.FindAsync(id);
            if (p == null)
            {
                return NotFound(new { mensagem = "Produto não encontrado." });
            }

            var trans = await _context.TransacaoInventario.Where(ti => ti.ProdutoId == id).ToListAsync();
            if (trans.Any())
            {
                _context.TransacaoInventario.RemoveRange(trans);
                await _context.SaveChangesAsync();
            }

            _context.Produto.Remove(p);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Produto excluído com sucesso." });
        }
    }
}

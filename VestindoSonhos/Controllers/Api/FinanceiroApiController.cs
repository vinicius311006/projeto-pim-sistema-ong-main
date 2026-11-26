using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    [ApiController]
    [Route("api/financeiro")]
    [Authorize(Roles = "Administrador")]
    public class FinanceiroApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;

        public FinanceiroApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<FinanceiroResumoDto>> ObterResumoFinanceiro()
        {
            // Carteiras e saldos
            var carteiras = await _context.Carteira
                .AsNoTracking()
                .Select(c => new CarteiraResumoDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Saldo = c.Saldo
                })
                .ToListAsync();

            var categorias = await _context.Categoria
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaFinanceiraDto
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .ToListAsync();

            var saldoTotalGeral = carteiras.Sum(c => c.Saldo);

            // Transações com suas razões e categorias
            var transacoesDb = await _context.TransacaoFinanceira
                .AsNoTracking()
                .Include(tf => tf.IdNavigation)
                    .ThenInclude(t => t.CategoriaNavigation)
                .Include(tf => tf.CarteiraNavigation)
                .OrderByDescending(tf => tf.IdNavigation.Hora)
                .ToListAsync();

            var totalEntradas = transacoesDb
                .Where(tf => tf.IdNavigation != null && tf.IdNavigation.Razao == "Entrada")
                .Sum(tf => tf.Valor);

            var totalSaidas = transacoesDb
                .Where(tf => tf.IdNavigation != null && (tf.IdNavigation.Razao == "Saida" || tf.IdNavigation.Razao == "Saída"))
                .Sum(tf => tf.Valor);

            var ultimasTransacoes = transacoesDb
                .Take(25)
                .Select(tf => new TransacaoFinanceiraDto
                {
                    Id = tf.Id,
                    Valor = tf.Valor,
                    Razao = tf.IdNavigation?.Razao ?? "Entrada",
                    CategoriaId = tf.IdNavigation?.Categoria,
                    Categoria = tf.IdNavigation?.CategoriaNavigation?.Nome ?? "Geral",
                    CarteiraId = tf.Carteira,
                    Carteira = tf.CarteiraNavigation?.Nome ?? "Principal",
                    Observacao = tf.IdNavigation?.Observacao,
                    DataHora = tf.IdNavigation?.Hora
                })
                .ToList();

            var resumo = new FinanceiroResumoDto
            {
                SaldoTotalGeral = saldoTotalGeral,
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                Carteiras = carteiras,
                Categorias = categorias,
                UltimasTransacoes = ultimasTransacoes
            };

            return Ok(resumo);
        }

        [HttpGet("categorias")]
        public async Task<ActionResult<IEnumerable<CategoriaFinanceiraDto>>> ObterCategorias()
        {
            var categorias = await _context.Categoria
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaFinanceiraDto
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpPost("categorias")]
        public async Task<ActionResult<CategoriaFinanceiraDto>> CriarCategoria([FromBody] CategoriaFinanceiraDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome da categoria é obrigatório." });
            }

            var nome = dto.Nome.Trim();
            if (nome.Length > 45) nome = nome.Substring(0, 45);

            var nova = new VestindoSonhos.Models.Categoria { Nome = nome };
            _context.Categoria.Add(nova);
            await _context.SaveChangesAsync();

            return Ok(new CategoriaFinanceiraDto { Id = nova.Id, Nome = nova.Nome });
        }

        [HttpPut("categorias/{id:int}")]
        public async Task<IActionResult> EditarCategoria(int id, [FromBody] CategoriaFinanceiraDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome da categoria é obrigatório." });
            }

            var cat = await _context.Categoria.FindAsync(id);
            if (cat == null)
            {
                return NotFound(new { mensagem = "Categoria financeira não encontrada." });
            }

            var nome = dto.Nome.Trim();
            if (nome.Length > 45) nome = nome.Substring(0, 45);

            cat.Nome = nome;
            _context.Categoria.Update(cat);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Categoria financeira atualizada com sucesso!" });
        }

        [HttpDelete("categorias/{id:int}")]
        public async Task<IActionResult> ExcluirCategoria(int id)
        {
            var cat = await _context.Categoria.FindAsync(id);
            if (cat == null)
            {
                return NotFound(new { mensagem = "Categoria financeira não encontrada." });
            }

            // Desvincula lançamentos existentes (deixando Sem Categoria) para permitir excluir inclusive a última categoria
            var transacoesVinculadas = await _context.Transacao.Where(t => t.Categoria == id).ToListAsync();
            if (transacoesVinculadas.Any())
            {
                foreach (var t in transacoesVinculadas)
                {
                    t.Categoria = null;
                }
                _context.Transacao.UpdateRange(transacoesVinculadas);
                await _context.SaveChangesAsync();
            }

            _context.Categoria.Remove(cat);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Categoria financeira excluída com sucesso!" });
        }

        [HttpDelete("categorias")]
        public async Task<IActionResult> ExcluirTodasCategorias()
        {
            var vinculadas = await _context.Transacao.Where(t => t.Categoria != null).ToListAsync();
            if (vinculadas.Any())
            {
                foreach (var t in vinculadas)
                {
                    t.Categoria = null;
                }
                _context.Transacao.UpdateRange(vinculadas);
                await _context.SaveChangesAsync();
            }

            var todas = await _context.Categoria.ToListAsync();
            if (todas.Any())
            {
                _context.Categoria.RemoveRange(todas);
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensagem = "Todas as categorias financeiras foram excluídas com sucesso!" });
        }

        [HttpPost("transacao")]
        public async Task<IActionResult> CriarTransacao([FromBody] CriarTransacaoFinanceiraDto dto)
        {
            if (dto.Valor <= 0 || dto.Valor > 99_999_999.99m)
            {
                return BadRequest(new { mensagem = "O valor da transação deve ser entre R$ 0,01 e R$ 99.999.999,99." });
            }

            var carteira = await _context.Carteira.FindAsync(dto.CarteiraId ?? 1);
            if (carteira == null)
            {
                carteira = await _context.Carteira.FirstOrDefaultAsync();
                if (carteira == null)
                {
                    return BadRequest(new { mensagem = "Nenhuma carteira financeira cadastrada." });
                }
            }

            var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int idUsuario = 1;
            if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
            {
                idUsuario = parsedId;
            }

            VestindoSonhos.Models.Categoria? categoria = null;
            if (dto.CategoriaId.HasValue && dto.CategoriaId.Value > 0)
            {
                categoria = await _context.Categoria.FindAsync(dto.CategoriaId.Value);
            }
            if (categoria == null && dto.CategoriaId == null)
            {
                categoria = await _context.Categoria.FirstOrDefaultAsync();
            }
            int? catId = categoria?.Id;

            var razao = ResolverRazaoFinanceira(dto.Razao, catId, categoria);

            var obs = dto.Observacao?.Trim();
            if (obs != null && obs.Length > 255) obs = obs.Substring(0, 255);

            var transacao = new Transacao
            {
                Usuario = idUsuario,
                Hora = DateTime.Now,
                Categoria = catId,
                Observacao = obs,
                Razao = razao
            };

            _context.Transacao.Add(transacao);
            await _context.SaveChangesAsync();

            var transacaoFin = new TransacaoFinanceira
            {
                Id = transacao.Id,
                Valor = dto.Valor,
                Carteira = carteira.Id
            };

            _context.TransacaoFinanceira.Add(transacaoFin);

            // Atualiza saldo da carteira
            if (razao == "Entrada")
            {
                carteira.Saldo += dto.Valor;
            }
            else
            {
                carteira.Saldo -= dto.Valor;
            }

            _context.Carteira.Update(carteira);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Transação financeira registrada com sucesso!" });
        }

        [HttpPut("transacao/{id:int}")]
        public async Task<IActionResult> EditarTransacao(int id, [FromBody] CriarTransacaoFinanceiraDto dto)
        {
            if (dto.Valor <= 0 || dto.Valor > 99_999_999.99m)
            {
                return BadRequest(new { mensagem = "O valor da transação deve ser entre R$ 0,01 e R$ 99.999.999,99." });
            }

            var tf = await _context.TransacaoFinanceira.FindAsync(id);
            var t = await _context.Transacao.FindAsync(id);
            if (tf == null || t == null)
            {
                return NotFound(new { mensagem = "Transação financeira não encontrada." });
            }

            // Reverte o saldo anterior na carteira antiga
            var carteiraAntiga = await _context.Carteira.FindAsync(tf.Carteira);
            var razaoAntiga = t.Razao ?? "Entrada";
            if (carteiraAntiga != null)
            {
                if (razaoAntiga == "Entrada")
                {
                    carteiraAntiga.Saldo -= tf.Valor;
                }
                else
                {
                    carteiraAntiga.Saldo += tf.Valor;
                }
                _context.Carteira.Update(carteiraAntiga);
            }

            var novaCarteiraId = dto.CarteiraId ?? tf.Carteira;
            var carteiraNova = await _context.Carteira.FindAsync(novaCarteiraId) ?? carteiraAntiga;

            int? novaCategoriaId = dto.CategoriaId ?? t.Categoria;
            var categoriaObj = (novaCategoriaId.HasValue && novaCategoriaId.Value > 0)
                ? await _context.Categoria.FindAsync(novaCategoriaId.Value)
                : null;
            t.Categoria = categoriaObj?.Id;

            var razaoNova = ResolverRazaoFinanceira(dto.Razao, t.Categoria, categoriaObj);
            t.Razao = razaoNova;

            if (dto.Observacao != null)
            {
                var obs = dto.Observacao.Trim();
                if (obs.Length > 255) obs = obs.Substring(0, 255);
                t.Observacao = obs;
            }

            tf.Valor = dto.Valor;
            if (carteiraNova != null)
            {
                tf.Carteira = carteiraNova.Id;
                if (razaoNova == "Entrada")
                {
                    carteiraNova.Saldo += dto.Valor;
                }
                else
                {
                    carteiraNova.Saldo -= dto.Valor;
                }
                _context.Carteira.Update(carteiraNova);
            }

            _context.Transacao.Update(t);
            _context.TransacaoFinanceira.Update(tf);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Transação financeira atualizada com sucesso!" });
        }

        [HttpDelete("transacao/{id:int}")]
        public async Task<IActionResult> ExcluirTransacao(int id)
        {
            var tf = await _context.TransacaoFinanceira
                .Include(x => x.IdNavigation)
                .Include(x => x.CarteiraNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tf == null)
            {
                return NotFound(new { mensagem = "Transação financeira não encontrada." });
            }

            // Reverte o saldo na carteira
            if (tf.CarteiraNavigation != null)
            {
                var razao = tf.IdNavigation?.Razao ?? "Entrada";
                if (razao == "Entrada")
                {
                    tf.CarteiraNavigation.Saldo -= tf.Valor;
                }
                else
                {
                    tf.CarteiraNavigation.Saldo += tf.Valor;
                }
                _context.Carteira.Update(tf.CarteiraNavigation);
            }

            var t = await _context.Transacao.FindAsync(id);

            _context.TransacaoFinanceira.Remove(tf);
            await _context.SaveChangesAsync();

            if (t != null)
            {
                _context.Transacao.Remove(t);
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensagem = "Transação excluída com sucesso." });
        }

        /// <summary>
        /// Garante que lançamentos na categoria Doação sejam sempre registrados como Entrada.
        /// </summary>
        private static string ResolverRazaoFinanceira(string? razaoInformada, int? catId, VestindoSonhos.Models.Categoria? categoria)
        {
            var nome = categoria?.Nome?.Trim();
            bool ehDoacao = (categoria != null && catId == 1)
                || string.Equals(nome, "Doação", StringComparison.OrdinalIgnoreCase)
                || string.Equals(nome, "Doacao", StringComparison.OrdinalIgnoreCase);

            if (ehDoacao)
            {
                return "Entrada";
            }

            return (razaoInformada == "Saída" || razaoInformada == "Saida") ? "Saída" : "Entrada";
        }
    }
}

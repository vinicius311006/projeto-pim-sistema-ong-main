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
                .Take(15)
                .Select(tf => new TransacaoFinanceiraDto
                {
                    Id = tf.Id,
                    Valor = tf.Valor,
                    Razao = tf.IdNavigation?.Razao ?? "Entrada",
                    Categoria = tf.IdNavigation?.CategoriaNavigation?.Nome ?? "Geral",
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
                UltimasTransacoes = ultimasTransacoes
            };

            return Ok(resumo);
        }
    }
}

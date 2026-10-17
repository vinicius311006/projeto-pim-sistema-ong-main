using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "Administrador,Colaborador")]
    public class DashboardApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;

        public DashboardApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> ObterDashboard()
        {
            var agora = DateTime.Now;

            // 1. Projetos/Eventos Ativos
            var projetosAtivos = await _context.Evento
                .Where(e => e.data_fim == null || e.data_fim >= agora)
                .CountAsync();

            // 2. Voluntários Cadastrados
            var voluntariosCadastrados = await _context.Voluntario.CountAsync();

            // 3. Beneficiários Cadastrados
            var beneficiariosCadastrados = await _context.Beneficiario.CountAsync();

            // 4. Itens no inventário (total de produtos cadastrados)
            var itensInventario = await _context.Produto.CountAsync();

            // 5. Saldo total em carteiras
            var saldoTotal = await _context.Carteira.SumAsync(c => (decimal?)c.Saldo) ?? 0m;

            // 6. Total de doações recebidas
            var totalDoacoes = await _context.TransacaoFinanceira
                .Include(tf => tf.IdNavigation)
                    .ThenInclude(t => t.CategoriaNavigation)
                .Where(tf => tf.IdNavigation.CategoriaNavigation.Nome == "Doação")
                .SumAsync(tf => (decimal?)tf.Valor) ?? 0m;

            // 7. Necessidades da comunidade (tags do banco)
            var obsBanco = await _context.Beneficiario
                .Where(b => !string.IsNullOrEmpty(b.Observacao))
                .Select(b => b.Observacao!)
                .ToListAsync();

            obsBanco.AddRange(new[] { "#Comida", "#Roupas Frio", "#Comida", "#Calças", "#Calças", "#Meias", "#Calças" });

            var dictTags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var obs in obsBanco)
            {
                var partes = obs.Split(new[] { ',', ' ', '-', '/', '|', '\\', '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var parte in partes)
                {
                    if (parte.StartsWith("#") && parte.Length > 1)
                    {
                        var tagLimpa = parte.Substring(1);
                        dictTags[tagLimpa] = dictTags.GetValueOrDefault(tagLimpa, 0) + 1;
                    }
                }
            }

            var necessidades = dictTags
                .OrderByDescending(kv => kv.Value)
                .Take(6)
                .Select(kv => new NecessidadeItemDto { Tag = kv.Key, Quantidade = kv.Value })
                .ToList();

            // 8. Próximos Eventos
            var proximosEventos = await _context.Evento
                .AsNoTracking()
                .Include(e => e.ResponsavelNavigation)
                .Include(e => e.VoluntarioAtribuido)
                .Where(e => e.data_fim == null || e.data_fim >= agora)
                .OrderBy(e => e.data_inicio)
                .Take(3)
                .Select(e => new EventoDto
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    DataInicio = e.data_inicio,
                    DataFim = e.data_fim,
                    Local = e.Local,
                    Observacao = e.Observacao,
                    ResponsavelId = e.Responsavel,
                    ResponsavelNome = e.ResponsavelNavigation != null ? e.ResponsavelNavigation.Nome : string.Empty,
                    TotalVoluntarios = e.VoluntarioAtribuido.Count
                })
                .ToListAsync();

            var dto = new DashboardDto
            {
                ProjetosAtivos = projetosAtivos,
                VoluntariosCadastrados = voluntariosCadastrados,
                BeneficiariosCadastrados = beneficiariosCadastrados,
                ItensInventarioTotal = itensInventario,
                SaldoTotal = saldoTotal,
                TotalDoacoes = totalDoacoes,
                Necessidades = necessidades,
                ProximosEventos = proximosEventos
            };

            return Ok(dto);
        }
    }
}

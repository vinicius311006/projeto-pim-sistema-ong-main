using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
        private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "saas-settings.json");

        public DashboardApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        private Definicoes.SaasConfigModel CarregarConfiguracoes()
        {
            try
            {
                if (System.IO.File.Exists(SettingsFilePath))
                {
                    var json = System.IO.File.ReadAllText(SettingsFilePath);
                    var cfg = JsonSerializer.Deserialize<Definicoes.SaasConfigModel>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { }
            return new Definicoes.SaasConfigModel();
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> ObterDashboard()
        {
            var agora = DateTime.Now;
            var config = CarregarConfiguracoes();

            // 1. Projetos/Eventos Ativos
            var projetosAtivos = await _context.Evento
                .Where(e => e.data_fim == null || e.data_fim >= agora)
                .CountAsync();

            // 2. Voluntários Cadastrados
            var voluntariosCadastrados = await _context.Voluntario.CountAsync();

            // 3. Beneficiários e Dependentes Cadastrados
            var beneficiariosCadastrados = await _context.Beneficiario.CountAsync();
            var totalDependentes = await _context.Dependente.CountAsync();

            // 4. Inventário (produtos, unidades em estoque e itens doados/saídas)
            var itensInventario = await _context.Produto.CountAsync();
            var totalUnidadesEstoque = await _context.Inventario.SumAsync(i => (int?)i.Quantidade) ?? 0;

            var transacoesInv = await _context.TransacaoInventario
                .Include(ti => ti.IdNavigation)
                    .ThenInclude(t => t.RazaoNavigation)
                .AsNoTracking()
                .ToListAsync();

            var totalItensDoadosSaida = transacoesInv
                .Where(ti => ti.IdNavigation?.RazaoNavigation?.Nome != null &&
                            (ti.IdNavigation.RazaoNavigation.Nome.Contains("Saída", StringComparison.OrdinalIgnoreCase) ||
                             ti.IdNavigation.RazaoNavigation.Nome.Contains("Saida", StringComparison.OrdinalIgnoreCase) ||
                             ti.IdNavigation.RazaoNavigation.Nome.Contains("Doação", StringComparison.OrdinalIgnoreCase)))
                .Sum(ti => ti.Quantidade);

            // 5. Distribuição do Inventário por Categoria
            var inventarioLista = await _context.Inventario
                .Include(i => i.ProdutoNavigation)
                    .ThenInclude(p => p.TipoAtivoNavigation)
                .AsNoTracking()
                .ToListAsync();

            var inventarioPorCategoria = inventarioLista
                .GroupBy(i => i.ProdutoNavigation?.TipoAtivoNavigation?.Nome ?? "Vestuário Geral")
                .Select(g => new CategoriaEstoqueMobileDto
                {
                    Categoria = g.Key,
                    Quantidade = g.Sum(x => x.Quantidade)
                })
                .OrderByDescending(g => g.Quantidade)
                .Take(6)
                .ToList();

            // 6. Financeiro (Saldo total, Entradas, Saídas, Doações e Últimas Transações)
            var saldoTotal = await _context.Carteira.SumAsync(c => (decimal?)c.Saldo) ?? 0m;

            var todasTransacoesFin = await _context.TransacaoFinanceira
                .Include(tf => tf.CarteiraNavigation)
                .Include(tf => tf.IdNavigation)
                    .ThenInclude(t => t.RazaoNavigation)
                .Include(tf => tf.IdNavigation)
                    .ThenInclude(t => t.CategoriaNavigation)
                .AsNoTracking()
                .OrderByDescending(tf => tf.Id)
                .ToListAsync();

            decimal entradasTotal = 0m;
            decimal saidasTotal = 0m;
            decimal totalDoacoes = 0m;

            foreach (var tf in todasTransacoesFin)
            {
                var razao = tf.IdNavigation?.RazaoNavigation?.Nome ?? "Entrada";
                var cat = tf.IdNavigation?.CategoriaNavigation?.Nome ?? "Geral";
                bool isEntrada = razao.Contains("Entrada", StringComparison.OrdinalIgnoreCase);

                if (isEntrada)
                {
                    entradasTotal += tf.Valor;
                    if (cat.Contains("Doação", StringComparison.OrdinalIgnoreCase) ||
                        cat.Contains("Doacao", StringComparison.OrdinalIgnoreCase))
                    {
                        totalDoacoes += tf.Valor;
                    }
                }
                else
                {
                    saidasTotal += tf.Valor;
                }
            }

            var ultimasTransacoes = todasTransacoesFin
                .Take(5)
                .Select(tf => new TransacaoResumoMobileDto
                {
                    Id = tf.Id,
                    Razao = tf.IdNavigation?.RazaoNavigation?.Nome ?? "Entrada",
                    Categoria = tf.IdNavigation?.CategoriaNavigation?.Nome ?? "Geral",
                    Carteira = tf.CarteiraNavigation?.Nome ?? "Principal",
                    Valor = tf.Valor,
                    Observacao = tf.IdNavigation?.Observacao
                })
                .ToList();

            // 7. Necessidades da comunidade (apenas tags reais do banco)
            var obsBanco = await _context.Beneficiario
                .Where(b => !string.IsNullOrEmpty(b.Observacao))
                .Select(b => b.Observacao!)
                .ToListAsync();

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
                    else if (parte.Length > 3)
                    {
                        dictTags[parte] = dictTags.GetValueOrDefault(parte, 0) + 1;
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
                .Take(5)
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
                TotalDependentes = totalDependentes,
                ItensInventarioTotal = itensInventario,
                TotalUnidadesEstoque = totalUnidadesEstoque,
                TotalItensDoadosSaida = totalItensDoadosSaida,
                SaldoTotal = saldoTotal,
                TotalDoacoes = totalDoacoes,
                EntradasMesAtual = entradasTotal,
                SaidasMesAtual = saidasTotal,
                DoacoesMesAtual = totalDoacoes,
                MetaArrecadacaoMensal = config.MetaArrecadacaoMensal,
                NomeOrganizacao = config.NomeOrganizacao,
                AlertaEstoqueMinimo = config.AlertaEstoqueMinimo,
                Necessidades = necessidades,
                InventarioPorCategoria = inventarioPorCategoria,
                UltimasTransacoes = ultimasTransacoes,
                ProximosEventos = proximosEventos
            };

            return Ok(dto);
        }

        [HttpGet("configuracoes")]
        [Authorize(Roles = "Administrador")]
        public ActionResult<Definicoes.SaasConfigModel> ObterConfiguracoesSaas()
        {
            return Ok(CarregarConfiguracoes());
        }

        [HttpPost("configuracoes")]
        [Authorize(Roles = "Administrador")]
        public IActionResult SalvarConfiguracoesSaas([FromBody] Definicoes.SaasConfigModel model)
        {
            if (model == null) return BadRequest(new { mensagem = "Dados inválidos." });

            try
            {
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(SettingsFilePath, json);
                return Ok(new { mensagem = "Configurações SaaS atualizadas com sucesso!", config = model });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao salvar configurações: " + ex.Message });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class DashboardViewModel
    {
        // =====================================================================
        // 1. INDICADORES PRINCIPAIS (KPI CARDS)
        // =====================================================================
        public int TotalBeneficiarios { get; set; }
        public int TotalDependentes { get; set; }
        public int TotalVoluntarios { get; set; }
        public int EventosAgendados { get; set; }
        public int TotalEventosRealizados { get; set; }

        public decimal SaldoAtual { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal TotalDoacoes { get; set; }
        public decimal DoacoesMesAtual { get; set; }
        public decimal EntradasMesAtual { get; set; }
        public decimal SaidasMesAtual { get; set; }

        public int TotalProdutosCadastrados { get; set; }
        public int TotalUnidadesEstoque { get; set; }
        public int TotalItensDoadosSaida { get; set; }

        // =====================================================================
        // 2. SÉRIES PARA OS GRÁFICOS (CHART.JS)
        // =====================================================================
        public List<string> MesesLabels { get; set; } = new();
        public List<decimal> EntradasPorMes { get; set; } = new();
        public List<decimal> SaidasPorMes { get; set; } = new();
        public List<decimal> DoacoesPorMes { get; set; } = new();

        public List<CategoriaQuantidadeItem> InventarioPorCategoria { get; set; } = new();
        public List<CategoriaQuantidadeItem> BeneficiariosPorSituacao { get; set; } = new();
        public List<CategoriaValorItem> MovimentacoesPorCategoriaFinanceira { get; set; } = new();
        public List<CategoriaQuantidadeItem> NecessidadesComunidade { get; set; } = new();

        // =====================================================================
        // 3. TABELAS DE ACESSO RÁPIDO
        // =====================================================================
        public List<TransacaoRecenteItem> UltimasTransacoes { get; set; } = new();
        public List<EventoProximoItem> ProximosEventos { get; set; } = new();
        public List<Carteira> Carteiras { get; set; } = new();

        // =====================================================================
        // 4. ESTRUTURAS AUXILIARES E COMPATIBILIDADE
        // =====================================================================
        public class CategoriaQuantidadeItem
        {
            public string Categoria { get; set; } = string.Empty;
            public int Quantidade { get; set; }
        }

        public class CategoriaValorItem
        {
            public string Categoria { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public int QuantidadeOperacoes { get; set; }
        }

        public class TransacaoRecenteItem
        {
            public int Id { get; set; }
            public DateTime? DataHora { get; set; }
            public string Razao { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public string Carteira { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string? Observacao { get; set; }
            public decimal Valor { get; set; }
        }

        public class EventoProximoItem
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public DateTime? DataInicio { get; set; }
            public DateTime? DataFim { get; set; }
            public string Local { get; set; } = string.Empty;
            public string Responsavel { get; set; } = string.Empty;
            public int TotalVoluntarios { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public class EstatisticasFinanceiras
        {
            public DateTime? Data { get; set; }
            public decimal Valor { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string Razao { get; set; } = string.Empty;
        }

        public class EstatisticasFinanceirasDoacao
        {
            public DateTime? Data { get; set; }
            public decimal Valor { get; set; }
        }

        public class EstatisticasDoacoesSaida
        {
            public int Quantidade { get; set; }
            public DateTime? Data { get; set; }
        }

        public class EstatisticasVoluntariosAtribuidos
        {
            public string nomeEvento { get; set; } = string.Empty;
            public int voluntariosAtribuidos { get; set; }
        }

        public List<EstatisticasVoluntariosAtribuidos> estatisticasVoluntariosAtribuidos { get; set; } = new();
        public List<EstatisticasFinanceiras> estatisticasFinanceiras { get; set; } = new();
        public List<EstatisticasFinanceirasDoacao> estatisticasFinanceirasDoacao { get; set; } = new();
        public List<EstatisticasDoacoesSaida> estatisticasRoupas { get; set; } = new();
    }
}

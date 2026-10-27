namespace VestindoSonhos.Models.DTOs
{
    public class DashboardDto
    {
        public int ProjetosAtivos { get; set; }
        public int VoluntariosCadastrados { get; set; }
        public int BeneficiariosCadastrados { get; set; }
        public int TotalDependentes { get; set; }
        public int ItensInventarioTotal { get; set; }
        public int TotalUnidadesEstoque { get; set; }
        public int TotalItensDoadosSaida { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal TotalDoacoes { get; set; }
        public decimal EntradasMesAtual { get; set; }
        public decimal SaidasMesAtual { get; set; }
        public decimal DoacoesMesAtual { get; set; }
        public decimal MetaArrecadacaoMensal { get; set; } = 15000.00m;
        public string NomeOrganizacao { get; set; } = "ONG Vestindo Sonhos";
        public int AlertaEstoqueMinimo { get; set; } = 10;
        public List<NecessidadeItemDto> Necessidades { get; set; } = new();
        public List<CategoriaEstoqueMobileDto> InventarioPorCategoria { get; set; } = new();
        public List<TransacaoResumoMobileDto> UltimasTransacoes { get; set; } = new();
        public List<EventoDto> ProximosEventos { get; set; } = new();
    }

    public class NecessidadeItemDto
    {
        public string Tag { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public class CategoriaEstoqueMobileDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public class TransacaoResumoMobileDto
    {
        public int Id { get; set; }
        public string Razao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Carteira { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Observacao { get; set; }
    }
}

namespace VestindoSonhos.Models.DTOs
{
    public class FinanceiroResumoDto
    {
        public decimal SaldoTotalGeral { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public List<CarteiraResumoDto> Carteiras { get; set; } = new();
        public List<TransacaoFinanceiraDto> UltimasTransacoes { get; set; } = new();
    }

    public class CarteiraResumoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
    }

    public class TransacaoFinanceiraDto
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public string Razao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Carteira { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public DateTime? DataHora { get; set; }
    }

    public class CriarTransacaoFinanceiraDto
    {
        public decimal Valor { get; set; }
        public string Razao { get; set; } = "Entrada"; // Entrada ou Saída
        public int? CarteiraId { get; set; }
        public int? CategoriaId { get; set; }
        public string? Observacao { get; set; }
    }
}

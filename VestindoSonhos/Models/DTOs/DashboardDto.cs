namespace VestindoSonhos.Models.DTOs
{
    public class DashboardDto
    {
        public int ProjetosAtivos { get; set; }
        public int VoluntariosCadastrados { get; set; }
        public int BeneficiariosCadastrados { get; set; }
        public int ItensInventarioTotal { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal TotalDoacoes { get; set; }
        public List<NecessidadeItemDto> Necessidades { get; set; } = new();
        public List<EventoDto> ProximosEventos { get; set; } = new();
    }

    public class NecessidadeItemDto
    {
        public string Tag { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }
}

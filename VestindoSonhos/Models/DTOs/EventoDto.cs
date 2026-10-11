using System;

namespace VestindoSonhos.Models.DTOs
{
    /// <summary>
    /// Modelo de dados (DTO) para transferência de dados de eventos na API REST.
    /// Evita referências circulares e otimiza a serialização JSON para o app mobile.
    /// </summary>
    public class EventoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? Local { get; set; }
        public string? Observacao { get; set; }
        public int ResponsavelId { get; set; }
        public string ResponsavelNome { get; set; } = string.Empty;
        public int TotalVoluntarios { get; set; }
    }
}

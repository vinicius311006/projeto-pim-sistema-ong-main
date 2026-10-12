using System;

namespace VestindoSonhos.Models.DTOs
{
    /// <summary>
    /// Modelo de dados retornado após autenticação JWT com sucesso.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime Expiration { get; set; }
        public UsuarioInfoDto Usuario { get; set; } = null!;
    }

    /// <summary>
    /// Dados resumidos do usuário autenticado para consumo pelo frontend/mobile.
    /// </summary>
    public class UsuarioInfoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public int? VoluntarioId { get; set; }
        public bool ExigirTrocaSenha { get; set; } = false;
    }
}

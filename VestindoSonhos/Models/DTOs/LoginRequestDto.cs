using System.ComponentModel.DataAnnotations;

namespace VestindoSonhos.Models.DTOs
{
    /// <summary>
    /// Modelo de dados para requisição de autenticação na API REST.
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para requisição de troca de senha obrigatória via mobile/API.
    /// </summary>
    public class TrocarSenhaObrigatoriaApiRequestDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }
}

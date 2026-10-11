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
}

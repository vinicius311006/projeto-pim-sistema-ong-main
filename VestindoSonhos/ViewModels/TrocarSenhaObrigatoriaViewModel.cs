using System.ComponentModel.DataAnnotations;

namespace VestindoSonhos.ViewModels
{
    public class TrocarSenhaObrigatoriaViewModel
    {
        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua nova senha pessoal.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua nova senha.")]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas digitadas não coincidem.")]
        [DataType(DataType.Password)]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}

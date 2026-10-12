using System.ComponentModel.DataAnnotations;

namespace VestindoSonhos.ViewModels
{
    public class RedefinirSenhaUsuarioViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? CargoNome { get; set; }

        [Required(ErrorMessage = "Informe a nova senha.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha.")]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas informadas não conferem.")]
        [DataType(DataType.Password)]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;

        [Display(Name = "Exigir troca de senha no próximo login")]
        public bool ExigirTrocaSenha { get; set; } = true;
    }
}

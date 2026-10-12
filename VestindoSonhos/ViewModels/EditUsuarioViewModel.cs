using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class EditUsuariosViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;

        public int Cargo { get; set; }

        public DateOnly? DataNascimento { get; set; }

        public string? Telefone { get; set; }

        public string? Email { get; set; }

        public string? Endereco { get; set; }

        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas informadas não conferem.")]
        public string? ConfirmarNovaSenha { get; set; }

        public virtual ICollection<Evento> Evento { get; set; } = new List<Evento>();

        public virtual ICollection<Transacao> Transacao { get; set; } = new List<Transacao>();
    }
}

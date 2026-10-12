using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class HomeUsuarioViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = null!;

        public int Cargo { get; set; }

        public string CargoNome { get; set; }

        public DateOnly? DataNascimento { get; set; }

        public string? Telefone { get; set; }

        public string? Email { get; set; }

        public string? Endereco { get; set; }

        public string Cpf { get; set; } = null!;

        public bool ExigirTrocaSenha { get; set; } = false;

        public virtual Cargo CargoNavigation { get; set; } = null!;

        public virtual ICollection<Evento> Evento { get; set; } = new List<Evento>();

        public virtual ICollection<Transacao> Transacao { get; set; } = new List<Transacao>();
    }
}

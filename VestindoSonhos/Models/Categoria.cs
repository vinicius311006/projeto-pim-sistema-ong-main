using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Transacao> Transacao { get; set; } = new List<Transacao>();
}

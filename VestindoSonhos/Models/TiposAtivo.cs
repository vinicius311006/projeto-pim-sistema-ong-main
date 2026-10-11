using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class TiposAtivo
{
    public int Id { get; set; }

    public string Categoria { get; set; } = null!;

    public virtual ICollection<Produto> Produto { get; set; } = new List<Produto>();
}

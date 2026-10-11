using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public string Cor { get; set; } = null!;

    public int? Categoria { get; set; }


    public virtual TiposAtivo? CategoriaNavigation { get; set; }
}

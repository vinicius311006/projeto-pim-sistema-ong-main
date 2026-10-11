using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Cargo
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();
}

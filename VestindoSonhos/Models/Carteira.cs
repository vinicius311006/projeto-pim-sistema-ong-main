using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Carteira
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal Saldo { get; set; }

    public virtual ICollection<TransacaoFinanceira> TransacaoFinanceira { get; set; } = new List<TransacaoFinanceira>();
}

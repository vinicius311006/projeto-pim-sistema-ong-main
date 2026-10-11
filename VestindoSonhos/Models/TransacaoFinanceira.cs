using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class TransacaoFinanceira
{
    public int Id { get; set; }

    public decimal Valor { get; set; }

    public int Carteira { get; set; }

    public virtual Carteira CarteiraNavigation { get; set; } = null!;

    public virtual Transacao IdNavigation { get; set; } = null!;
}

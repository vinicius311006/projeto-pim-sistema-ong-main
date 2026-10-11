using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;


public partial class Transacao
{
    public enum TipoTransacao
    {
        Entrada,
        Saída
    }
    public int Id { get; set; }

    public DateTime? Hora { get; set; }

    public int Usuario { get; set; }

    public string? Observacao { get; set; }

    public int Categoria { get; set; }

    public string? Razao { get; set; }

    public virtual Categoria CategoriaNavigation { get; set; } = null!;

    public virtual TransacaoFinanceira? TransacaoFinanceira { get; set; }

    public virtual Usuario UsuarioNavigation { get; set; } = null!;
}

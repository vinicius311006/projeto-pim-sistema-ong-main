using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Beneficiario
{
    public Beneficiario()
    {
        Dependente = new HashSet<Dependente>();
        TransacaoInventario = new HashSet<TransacaoInventario>();
    }

    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Telefone { get; set; } = null!;

    public string Endereco { get; set; } = null!;

    public string? Situacao { get; set; }

    public string? Observacao { get; set; }

    public virtual ICollection<Dependente> Dependente { get; set; }

    public virtual ICollection<TransacaoInventario> TransacaoInventario { get; set; }
}

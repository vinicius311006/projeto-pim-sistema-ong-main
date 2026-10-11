using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VestindoSonhos.Models;

public partial class TransacaoInventario
{
    public int ProdutoId { get; set; }

    public int TransacaoId { get; set; }

    public int Quantidade { get; set; }

    public int? Beneficiario { get; set; }
    public virtual Produto ProdutoNavigation { get; set; }

    [ForeignKey("Beneficiario")]
    [InverseProperty("TransacaoInventario")]
    public virtual Beneficiario? BeneficiarioNavigation { get; set; }
}

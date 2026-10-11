using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Dependente
{
    public int Id { get; set; }

    public int IdResponsavel { get; set; }

    public string Nome { get; set; } = null!;

    public int Idade { get; set; }

    public string Sexo { get; set; } = null!;

    public string? Parentesco { get; set; }

    public string? Observacao { get; set; }
    [ValidateNever]
    public virtual Beneficiario IdResponsavelNavigation { get; set; } = null!;
}

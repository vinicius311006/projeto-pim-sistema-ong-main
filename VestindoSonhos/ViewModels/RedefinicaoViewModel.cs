using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace VestindoSonhos.ViewModels;

public partial class RedefinicaoViewModel
{
    public int Id { get; set; }

    public string SenhaAtual { get; set; } = null!;

    public string SenhaNova { get; set; } = null!;

    public string RepeteSenhaNova { get; set; } = null!;
}

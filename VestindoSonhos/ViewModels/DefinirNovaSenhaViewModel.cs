using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;

namespace VestindoSonhos.ViewModels;

public partial class DefinirNovaSenhaViewModel
{
    public string Email { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public string SenhaNova { get; set; } = null!;

    public string RepeteSenhaNova { get; set; } = null!;
}

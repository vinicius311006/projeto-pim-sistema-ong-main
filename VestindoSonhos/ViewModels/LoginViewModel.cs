using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace VestindoSonhos.ViewModels;

public partial class LoginViewModel
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string Senha { get; set; } = null!;
}

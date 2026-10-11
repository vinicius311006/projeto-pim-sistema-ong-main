using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace VestindoSonhos.ViewModels;

public partial class UsuarioViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;

    public int Cargo { get; set; } = 1;

    public DateOnly? DataNascimento { get; set; }

    public string? Telefone { get; set; }

    public string Email { get; set; }

    public string? Endereco { get; set; }

    public string Cpf { get; set; } = null!;

    public string Senha { get; set; } = null!;
}

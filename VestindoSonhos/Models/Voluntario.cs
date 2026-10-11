using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Voluntario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public string? Endereco { get; set; }

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<VoluntarioAtribuido> VoluntarioAtribuido { get; set; } = new List<VoluntarioAtribuido>();
}

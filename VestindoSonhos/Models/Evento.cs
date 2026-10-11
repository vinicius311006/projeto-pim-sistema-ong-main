using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class Evento
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public DateTime? data_inicio { get; set; }

    public DateTime? data_fim { get; set; }

    public string? Local { get; set; }

    public int Responsavel { get; set; }

    public string? Observacao { get; set; }

    public virtual Usuario ResponsavelNavigation { get; set; } = null!;

    public virtual ICollection<VoluntarioAtribuido> VoluntarioAtribuido { get; set; } = new List<VoluntarioAtribuido>();
}

using System;
using System.Collections.Generic;

namespace VestindoSonhos.Models;

public partial class VoluntarioAtribuido
{
    public int Evento { get; set; }

    public int Voluntario { get; set; }

    public DateTime DataAdicao { get; set; }

    public virtual Evento EventoNavigation { get; set; } = null!;

    public virtual Voluntario VoluntarioNavigation { get; set; } = null!;
}

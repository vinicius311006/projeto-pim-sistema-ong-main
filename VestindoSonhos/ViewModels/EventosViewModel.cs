using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels;

public partial class EventosViewModel
{
    public List<Evento> Eventos { get; set; }

    public List<Voluntario>  Voluntarios { get; set; }
}

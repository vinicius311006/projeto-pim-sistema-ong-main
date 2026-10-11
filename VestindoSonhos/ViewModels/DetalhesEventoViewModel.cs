using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class DetalhesEventoViewModel
    {

        public List<Voluntario> listaVoluntarios { get; set; }

        public Evento evento { get; set; }

        public List<int> idVoluntarios { get; set; } = new();


    }
}

using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class HomeViewModel
    {
        public Dictionary<string, int> necessidades { get; set; }
        public int projetosAtivos { get; set; }

        public int eventosPendentesMesVigente { get; set; }

        public int voluntariosCadastrados { get; set; }

        public Evento ultimoEvento { get; set; }

        public List<Evento> proximosEventos { get; set; }
    }
}
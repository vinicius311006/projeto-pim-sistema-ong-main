using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class CriacaoEventoViewModel
    {
        

        public class VoluntarioView
        {
            public int Id { get; set; }

            public string Nome { get; set; }

            public string Telefone { get; set; }

            public virtual ICollection<VoluntarioView> IdCollection { get; set; } = new List<VoluntarioView>();
        }

        public Evento evento { get; set; }

        public List<int> idVoluntarios { get; set; } = new();
        public List<VoluntarioView>? listaVoluntarios { get; set; }


    }
}

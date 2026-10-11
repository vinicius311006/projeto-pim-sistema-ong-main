using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VestindoSonhos.Controllers;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{

    public partial class InventarioViewModel
    {   
        public string nome_produto { get; set; }

        public int id_produto { get; set; }

        public int transacao_id { get; set; }

        public int quantidade { get; set; }

        public string beneficiario { get; set; }

        public Transacao.TipoTransacao razao { get; set; }

        public DateTime? hora { get; set; }

        public string observacao { get; set; }

        public string categoria { get; set; }

        public string usuario { get; set; }
    }
}
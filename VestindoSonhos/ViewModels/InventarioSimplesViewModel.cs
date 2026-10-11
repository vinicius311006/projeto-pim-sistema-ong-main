using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VestindoSonhos.Controllers;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class InventarioSimplesViewModel
    {          
        public int TransacaoId { get; set; }
        public int Id { get; set; }
        public string NomeProduto { get; set; }

        public int Quantidade { get; set; }

        public string Razao { get; set; }

        public DateTime? Hora { get; set; }
    }
}
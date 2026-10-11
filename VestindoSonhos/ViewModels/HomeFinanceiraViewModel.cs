using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VestindoSonhos.Controllers;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class HomeFinanceiraViewModel
    {
        public List<Carteira> lista_carteiras { get; set; }
        public List<TransacaoFinanceiraViewModel> lista_transacoes { get; set; }

        public List<TransacaoFinanceiraViewModel> lista_doacoes { get; set; }

    }
}
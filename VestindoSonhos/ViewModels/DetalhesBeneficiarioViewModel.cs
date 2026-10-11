using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class DetalhesBeneficiarioViewModel
    {
        
        public Beneficiario beneficiario { get; set; }

        public List<Beneficiario> listaBeneficiario { get; set; } = null!;

        public List<Dependente> listaDependentes { get; set; } = null!;
    }
}

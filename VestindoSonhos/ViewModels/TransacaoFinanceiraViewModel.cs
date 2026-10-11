using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VestindoSonhos.ViewModels
{
    public partial class TransacaoFinanceiraViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O valor é obrigatório")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Escolha uma carteira válida")]
        public int Carteira { get; set; }
        [ValidateNever]
        public string Usuario { get; set; } = null!;
        [ValidateNever]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Defina uma razão")]
        public string Razao { get; set; } = null!;

        [Required(ErrorMessage = "Selecione uma categoria")]
        public int Categoria { get; set; }

        public string? Observacao { get; set; }

        [Required(ErrorMessage = "Qual a data da transação?")]
        public DateTime? Hora { get; set; }
        [ValidateNever]
        public virtual Models.Categoria CategoriaNavigation { get; set; } = null!;
    }
}
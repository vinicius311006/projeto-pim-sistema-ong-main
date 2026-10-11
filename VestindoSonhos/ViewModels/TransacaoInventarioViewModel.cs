using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class TransacaoInventarioViewModel
    {
        public class ItemTransacao
        {
            public int ProdutoId { get; set; }
            public bool Selecionado { get; set; }
            public int Quantidade { get; set; }
            public int? Categoria { get; set; }
            public Produto ProdutoNavigation { get; set; }

            public TiposAtivo? CategoriaNavigation { get; set; }
        }
        public int IdTransacao { get; set; }

        public DateTime? Hora { get; set; }

        public int Usuario { get; set; }

        public string? Observacao { get; set; }

        public int Categoria { get; set; }

        public int? Beneficiario { get; set; }

        public string Razao { get; set; }


        [ForeignKey("Beneficiario")]
        public virtual Beneficiario BeneficiarioNavigation { get; set; } = null!;

        public List<ItemTransacao> listaItensAtribuidos { get; set; } = new();

        public virtual Categoria CategoriaNavigation { get; set; } = null!;

        public virtual Produto ProdutoNavigation { get; set; } = null!;

        public virtual TransacaoFinanceira? TransacaoFinanceira { get; set; }

        public virtual Usuario UsuarioNavigation { get; set; } = null!;

        public virtual List<Produto> listaTodosProdutos { get; set; } = new();
    }
}
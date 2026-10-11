using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class TransacaoInventarioCriacaoEdicaoViewModel
    {
        public class ItemTransacaoCriacaoEdicao
        {
            public int ProdutoId { get; set; }

            public string? NomeProduto { get; set; }

            public string? Descricao { get; set; }

            public string? Cor { get; set; }

            public bool Selecionado { get; set; }

            public int Quantidade { get; set; }

            public int? CategoriaProduto { get; set; }

            public string? NomeCategoriaProduto { get; set; }
        }
        public int IdTransacao { get; set; }
        public int CategoriaTransacao { get; set; }

        public string? NomeCategoriaTransacao { get; set; }

        public int? Beneficiario { get; set; }

        public string? NomeBeneficiario { get; set; }

        public string? Observacao { get; set; }

        public string? Razao { get; set; }
        
        public List<ItemTransacaoCriacaoEdicao> Produtos { get; set; } = new();
    }
}
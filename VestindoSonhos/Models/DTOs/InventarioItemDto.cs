namespace VestindoSonhos.Models.DTOs
{
    public class InventarioItemDto
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int QuantidadeEmEstoque { get; set; }
    }

    public class CategoriaInventarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class CriarMovimentacaoDto
    {
        public string Razao { get; set; } = "Entrada"; // Entrada ou Saída
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; } = 1;
        public int? BeneficiarioId { get; set; }
        public string? Observacao { get; set; }
    }

    public class CriarProdutoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
    }
}

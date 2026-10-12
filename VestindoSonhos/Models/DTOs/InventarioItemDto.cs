namespace VestindoSonhos.Models.DTOs
{
    public class InventarioItemDto
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int QuantidadeEmEstoque { get; set; }
    }
}

namespace VestindoSonhos.Models.DTOs
{
    public class BeneficiarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public int TotalDependentes { get; set; }
    }

    public class CriarEditarBeneficiarioDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Situacao { get; set; } = "Ok";
        public string? Observacao { get; set; }
    }
}

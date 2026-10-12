class InventarioModel {
  final int produtoId;
  final String nomeProduto;
  final String descricao;
  final String cor;
  final String categoria;
  final int quantidadeEmEstoque;

  InventarioModel({
    required this.produtoId,
    required this.nomeProduto,
    required this.descricao,
    required this.cor,
    required this.categoria,
    required this.quantidadeEmEstoque,
  });

  factory InventarioModel.fromJson(Map<String, dynamic> json) {
    return InventarioModel(
      produtoId: json['produtoId'] ?? 0,
      nomeProduto: json['nomeProduto'] ?? '',
      descricao: json['descricao'] ?? '',
      cor: json['cor'] ?? '',
      categoria: json['categoria'] ?? 'Geral',
      quantidadeEmEstoque: json['quantidadeEmEstoque'] ?? 0,
    );
  }
}

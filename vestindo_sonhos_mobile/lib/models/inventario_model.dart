class InventarioModel {
  final int produtoId;
  final String nomeProduto;
  final String descricao;
  final String cor;
  final int? categoriaId;
  final String categoria;
  final int quantidadeEmEstoque;

  InventarioModel({
    required this.produtoId,
    required this.nomeProduto,
    required this.descricao,
    required this.cor,
    this.categoriaId,
    required this.categoria,
    required this.quantidadeEmEstoque,
  });

  factory InventarioModel.fromJson(Map<String, dynamic> json) {
    return InventarioModel(
      produtoId: json['produtoId'] ?? 0,
      nomeProduto: json['nomeProduto'] ?? '',
      descricao: json['descricao'] ?? '',
      cor: json['cor'] ?? '',
      categoriaId: json['categoriaId'] as int?,
      categoria: json['categoria'] ?? 'Geral',
      quantidadeEmEstoque: json['quantidadeEmEstoque'] ?? 0,
    );
  }
}

class CategoriaInventarioModel {
  final int id;
  final String nome;

  CategoriaInventarioModel({
    required this.id,
    required this.nome,
  });

  factory CategoriaInventarioModel.fromJson(Map<String, dynamic> json) {
    return CategoriaInventarioModel(
      id: json['id'] ?? 0,
      nome: json['nome'] ?? '',
    );
  }
}

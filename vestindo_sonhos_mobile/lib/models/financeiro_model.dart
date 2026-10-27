class FinanceiroModel {
  final double saldoTotalGeral;
  final double totalEntradas;
  final double totalSaidas;
  final List<CarteiraModel> carteiras;
  final List<CategoriaFinanceiraModel> categorias;
  final List<TransacaoModel> ultimasTransacoes;

  FinanceiroModel({
    required this.saldoTotalGeral,
    required this.totalEntradas,
    required this.totalSaidas,
    required this.carteiras,
    required this.categorias,
    required this.ultimasTransacoes,
  });

  factory FinanceiroModel.fromJson(Map<String, dynamic> json) {
    return FinanceiroModel(
      saldoTotalGeral: (json['saldoTotalGeral'] as num?)?.toDouble() ?? 0.0,
      totalEntradas: (json['totalEntradas'] as num?)?.toDouble() ?? 0.0,
      totalSaidas: (json['totalSaidas'] as num?)?.toDouble() ?? 0.0,
      carteiras: (json['carteiras'] as List<dynamic>?)
              ?.map((item) => CarteiraModel.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
      categorias: (json['categorias'] as List<dynamic>?)
              ?.map((item) => CategoriaFinanceiraModel.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
      ultimasTransacoes: (json['ultimasTransacoes'] as List<dynamic>?)
              ?.map((item) => TransacaoModel.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class CarteiraModel {
  final int id;
  final String nome;
  final double saldo;

  CarteiraModel({
    required this.id,
    required this.nome,
    required this.saldo,
  });

  factory CarteiraModel.fromJson(Map<String, dynamic> json) {
    return CarteiraModel(
      id: json['id'] ?? 0,
      nome: json['nome'] ?? '',
      saldo: (json['saldo'] as num?)?.toDouble() ?? 0.0,
    );
  }
}

class CategoriaFinanceiraModel {
  final int id;
  final String nome;

  CategoriaFinanceiraModel({
    required this.id,
    required this.nome,
  });

  factory CategoriaFinanceiraModel.fromJson(Map<String, dynamic> json) {
    return CategoriaFinanceiraModel(
      id: json['id'] ?? 0,
      nome: json['nome'] ?? '',
    );
  }
}

class TransacaoModel {
  final int id;
  final double valor;
  final String razao;
  final int? categoriaId;
  final String categoria;
  final int? carteiraId;
  final String carteira;
  final String? observacao;
  final DateTime? dataHora;

  TransacaoModel({
    required this.id,
    required this.valor,
    required this.razao,
    this.categoriaId,
    required this.categoria,
    this.carteiraId,
    required this.carteira,
    this.observacao,
    this.dataHora,
  });

  factory TransacaoModel.fromJson(Map<String, dynamic> json) {
    return TransacaoModel(
      id: json['id'] ?? 0,
      valor: (json['valor'] as num?)?.toDouble() ?? 0.0,
      razao: json['razao'] ?? 'Entrada',
      categoriaId: json['categoriaId'] as int?,
      categoria: json['categoria'] ?? 'Geral',
      carteiraId: json['carteiraId'] as int?,
      carteira: json['carteira'] ?? 'Principal',
      observacao: json['observacao'],
      dataHora: json['dataHora'] != null ? DateTime.tryParse(json['dataHora']) : null,
    );
  }
}

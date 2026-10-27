import 'evento_model.dart';

class NecessidadeItem {
  final String tag;
  final int quantidade;

  NecessidadeItem({required this.tag, required this.quantidade});

  factory NecessidadeItem.fromJson(Map<String, dynamic> json) {
    return NecessidadeItem(
      tag: (json['tag'] ?? json['Tag'] ?? '').toString(),
      quantidade: _parseInt(json['quantidade'] ?? json['Quantidade']),
    );
  }

  static int _parseInt(dynamic val) {
    if (val == null) return 0;
    if (val is int) return val;
    return int.tryParse(val.toString()) ?? 0;
  }
}

class CategoriaEstoqueItem {
  final String categoria;
  final int quantidade;

  CategoriaEstoqueItem({required this.categoria, required this.quantidade});

  factory CategoriaEstoqueItem.fromJson(Map<String, dynamic> json) {
    return CategoriaEstoqueItem(
      categoria: (json['categoria'] ?? json['Categoria'] ?? 'Geral').toString(),
      quantidade: NecessidadeItem._parseInt(json['quantidade'] ?? json['Quantidade']),
    );
  }
}

class TransacaoResumoItem {
  final int id;
  final String razao;
  final String categoria;
  final String carteira;
  final double valor;
  final String? observacao;

  TransacaoResumoItem({
    required this.id,
    required this.razao,
    required this.categoria,
    required this.carteira,
    required this.valor,
    this.observacao,
  });

  factory TransacaoResumoItem.fromJson(Map<String, dynamic> json) {
    return TransacaoResumoItem(
      id: NecessidadeItem._parseInt(json['id'] ?? json['Id']),
      razao: (json['razao'] ?? json['Razao'] ?? 'Entrada').toString(),
      categoria: (json['categoria'] ?? json['Categoria'] ?? 'Geral').toString(),
      carteira: (json['carteira'] ?? json['Carteira'] ?? 'Principal').toString(),
      valor: DashboardModel._parseDouble(json['valor'] ?? json['Valor']),
      observacao: (json['observacao'] ?? json['Observacao'])?.toString(),
    );
  }
}

class DashboardModel {
  final int projetosAtivos;
  final int voluntariosCadastrados;
  final int beneficiariosCadastrados;
  final int totalDependentes;
  final int itensInventarioTotal;
  final int totalUnidadesEstoque;
  final int totalItensDoadosSaida;
  final double saldoTotal;
  final double totalDoacoes;
  final double entradasMesAtual;
  final double saidasMesAtual;
  final double doacoesMesAtual;
  final double metaArrecadacaoMensal;
  final String nomeOrganizacao;
  final int alertaEstoqueMinimo;
  final List<NecessidadeItem> necessidades;
  final List<CategoriaEstoqueItem> inventarioPorCategoria;
  final List<TransacaoResumoItem> ultimasTransacoes;
  final List<EventoModel> proximosEventos;

  DashboardModel({
    required this.projetosAtivos,
    required this.voluntariosCadastrados,
    required this.beneficiariosCadastrados,
    required this.totalDependentes,
    required this.itensInventarioTotal,
    required this.totalUnidadesEstoque,
    required this.totalItensDoadosSaida,
    required this.saldoTotal,
    required this.totalDoacoes,
    required this.entradasMesAtual,
    required this.saidasMesAtual,
    required this.doacoesMesAtual,
    required this.metaArrecadacaoMensal,
    required this.nomeOrganizacao,
    required this.alertaEstoqueMinimo,
    required this.necessidades,
    required this.inventarioPorCategoria,
    required this.ultimasTransacoes,
    required this.proximosEventos,
  });

  factory DashboardModel.fromJson(Map<String, dynamic> json) {
    final necList = (json['necessidades'] ?? json['Necessidades'] ?? []) as List<dynamic>;
    final catList = (json['inventarioPorCategoria'] ?? json['InventarioPorCategoria'] ?? []) as List<dynamic>;
    final transList = (json['ultimasTransacoes'] ?? json['UltimasTransacoes'] ?? []) as List<dynamic>;
    final evList = (json['proximosEventos'] ?? json['ProximosEventos'] ?? []) as List<dynamic>;

    return DashboardModel(
      projetosAtivos: _parseInt(json['projetosAtivos'] ?? json['ProjetosAtivos']),
      voluntariosCadastrados: _parseInt(json['voluntariosCadastrados'] ?? json['VoluntariosCadastrados']),
      beneficiariosCadastrados: _parseInt(json['beneficiariosCadastrados'] ?? json['BeneficiariosCadastrados']),
      totalDependentes: _parseInt(json['totalDependentes'] ?? json['TotalDependentes']),
      itensInventarioTotal: _parseInt(json['itensInventarioTotal'] ?? json['ItensInventarioTotal']),
      totalUnidadesEstoque: _parseInt(json['totalUnidadesEstoque'] ?? json['TotalUnidadesEstoque']),
      totalItensDoadosSaida: _parseInt(json['totalItensDoadosSaida'] ?? json['TotalItensDoadosSaida']),
      saldoTotal: _parseDouble(json['saldoTotal'] ?? json['SaldoTotal']),
      totalDoacoes: _parseDouble(json['totalDoacoes'] ?? json['TotalDoacoes']),
      entradasMesAtual: _parseDouble(json['entradasMesAtual'] ?? json['EntradasMesAtual']),
      saidasMesAtual: _parseDouble(json['saidasMesAtual'] ?? json['SaidasMesAtual']),
      doacoesMesAtual: _parseDouble(json['doacoesMesAtual'] ?? json['DoacoesMesAtual']),
      metaArrecadacaoMensal: _parseDouble(json['metaArrecadacaoMensal'] ?? json['MetaArrecadacaoMensal'] ?? 15000),
      nomeOrganizacao: (json['nomeOrganizacao'] ?? json['NomeOrganizacao'] ?? 'ONG Vestindo Sonhos').toString(),
      alertaEstoqueMinimo: _parseInt(json['alertaEstoqueMinimo'] ?? json['AlertaEstoqueMinimo'] ?? 10),
      necessidades: necList.map((e) => NecessidadeItem.fromJson(e as Map<String, dynamic>)).toList(),
      inventarioPorCategoria: catList.map((e) => CategoriaEstoqueItem.fromJson(e as Map<String, dynamic>)).toList(),
      ultimasTransacoes: transList.map((e) => TransacaoResumoItem.fromJson(e as Map<String, dynamic>)).toList(),
      proximosEventos: evList.map((e) => EventoModel.fromJson(e as Map<String, dynamic>)).toList(),
    );
  }

  static int _parseInt(dynamic val) {
    if (val == null) return 0;
    if (val is int) return val;
    return int.tryParse(val.toString()) ?? 0;
  }

  static double _parseDouble(dynamic val) {
    if (val == null) return 0.0;
    if (val is num) return val.toDouble();
    return double.tryParse(val.toString()) ?? 0.0;
  }
}

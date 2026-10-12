import 'evento_model.dart';

class DashboardModel {
  final int projetosAtivos;
  final int voluntariosCadastrados;
  final int beneficiariosCadastrados;
  final int itensInventarioTotal;
  final double saldoTotal;
  final double totalDoacoes;
  final List<NecessidadeTagModel> necessidades;
  final List<EventoModel> proximosEventos;

  DashboardModel({
    required this.projetosAtivos,
    required this.voluntariosCadastrados,
    required this.beneficiariosCadastrados,
    required this.itensInventarioTotal,
    required this.saldoTotal,
    required this.totalDoacoes,
    required this.necessidades,
    required this.proximosEventos,
  });

  factory DashboardModel.fromJson(Map<String, dynamic> json) {
    return DashboardModel(
      projetosAtivos: json['projetosAtivos'] ?? 0,
      voluntariosCadastrados: json['voluntariosCadastrados'] ?? 0,
      beneficiariosCadastrados: json['beneficiariosCadastrados'] ?? 0,
      itensInventarioTotal: json['itensInventarioTotal'] ?? 0,
      saldoTotal: (json['saldoTotal'] as num?)?.toDouble() ?? 0.0,
      totalDoacoes: (json['totalDoacoes'] as num?)?.toDouble() ?? 0.0,
      necessidades: (json['necessidades'] as List<dynamic>?)
              ?.map((item) => NecessidadeTagModel.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
      proximosEventos: (json['proximosEventos'] as List<dynamic>?)
              ?.map((item) => EventoModel.fromJson(item as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class NecessidadeTagModel {
  final String tag;
  final int quantidade;

  NecessidadeTagModel({
    required this.tag,
    required this.quantidade,
  });

  factory NecessidadeTagModel.fromJson(Map<String, dynamic> json) {
    return NecessidadeTagModel(
      tag: json['tag'] ?? '',
      quantidade: json['quantidade'] ?? 0,
    );
  }
}

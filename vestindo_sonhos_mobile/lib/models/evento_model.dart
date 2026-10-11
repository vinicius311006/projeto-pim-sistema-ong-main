import 'package:intl/intl.dart';

/// Modelo de dados que representa um Evento da ONG Vestindo Sonhos.
/// Mapeia o JSON retornado pelo endpoint GET /api/eventos da API REST backend.
class EventoModel {
  final int id;
  final String nome;
  final DateTime? dataInicio;
  final DateTime? dataFim;
  final String? local;
  final String? observacao;
  final int responsavelId;
  final String responsavelNome;
  final int totalVoluntarios;

  EventoModel({
    required this.id,
    required this.nome,
    this.dataInicio,
    this.dataFim,
    this.local,
    this.observacao,
    required this.responsavelId,
    required this.responsavelNome,
    required this.totalVoluntarios,
  });

  /// Fábrica que converte o mapa JSON recebido da API em uma instância de EventoModel.
  factory EventoModel.fromJson(Map<String, dynamic> json) {
    return EventoModel(
      id: json['id'] is int ? json['id'] : int.tryParse(json['id']?.toString() ?? '0') ?? 0,
      nome: json['nome']?.toString() ?? 'Evento Sem Título',
      dataInicio: json['dataInicio'] != null 
          ? DateTime.tryParse(json['dataInicio'].toString()) 
          : null,
      dataFim: json['dataFim'] != null 
          ? DateTime.tryParse(json['dataFim'].toString()) 
          : null,
      local: json['local']?.toString(),
      observacao: json['observacao']?.toString(),
      responsavelId: json['responsavelId'] is int 
          ? json['responsavelId'] 
          : int.tryParse(json['responsavelId']?.toString() ?? '0') ?? 0,
      responsavelNome: json['responsavelNome']?.toString() ?? 'Responsável não informado',
      totalVoluntarios: json['totalVoluntarios'] is int 
          ? json['totalVoluntarios'] 
          : int.tryParse(json['totalVoluntarios']?.toString() ?? '0') ?? 0,
    );
  }

  /// Converte a instância em mapa JSON.
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'nome': nome,
      'dataInicio': dataInicio?.toIso8601String(),
      'dataFim': dataFim?.toIso8601String(),
      'local': local,
      'observacao': observacao,
      'responsavelId': responsavelId,
      'responsavelNome': responsavelNome,
      'totalVoluntarios': totalVoluntarios,
    };
  }

  /// Formatação amigável da data de início para exibição no aplicativo.
  String get dataInicioFormatada {
    if (dataInicio == null) return 'Data não definida';
    final formato = DateFormat('dd/MM/yyyy HH:mm');
    return formato.format(dataInicio!);
  }

  /// Formatação amigável da data de término para exibição no aplicativo.
  String get dataFimFormatada {
    if (dataFim == null) return 'Data não definida';
    final formato = DateFormat('dd/MM/yyyy HH:mm');
    return formato.format(dataFim!);
  }

  /// Retorna o período do evento formatado (Ex: "10/11/2026 até 12/11/2026").
  String get periodoFormatado {
    if (dataInicio == null) return 'Período a definir';
    final formatoData = DateFormat('dd/MM/yyyy');
    if (dataFim == null) return formatoData.format(dataInicio!);
    return '${formatoData.format(dataInicio!)} até ${formatoData.format(dataFim!)}';
  }
}

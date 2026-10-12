class BeneficiarioModel {
  final int id;
  final String nome;
  final String telefone;
  final String endereco;
  final String situacao;
  final String? observacao;
  final int totalDependentes;

  BeneficiarioModel({
    required this.id,
    required this.nome,
    required this.telefone,
    required this.endereco,
    required this.situacao,
    this.observacao,
    required this.totalDependentes,
  });

  factory BeneficiarioModel.fromJson(Map<String, dynamic> json) {
    return BeneficiarioModel(
      id: json['id'] ?? 0,
      nome: json['nome'] ?? '',
      telefone: json['telefone'] ?? '',
      endereco: json['endereco'] ?? '',
      situacao: json['situacao'] ?? 'Ok',
      observacao: json['observacao'],
      totalDependentes: json['totalDependentes'] ?? 0,
    );
  }
}

class VoluntarioModel {
  final int id;
  final String nome;
  final String cpf;
  final String endereco;
  final String telefone;
  final String email;
  final int totalEventos;

  VoluntarioModel({
    required this.id,
    required this.nome,
    required this.cpf,
    required this.endereco,
    required this.telefone,
    required this.email,
    required this.totalEventos,
  });

  factory VoluntarioModel.fromJson(Map<String, dynamic> json) {
    return VoluntarioModel(
      id: _parseInt(json['id'] ?? json['Id']),
      nome: (json['nome'] ?? json['Nome'] ?? '').toString(),
      cpf: (json['cpf'] ?? json['Cpf'] ?? '').toString(),
      endereco: (json['endereco'] ?? json['Endereco'] ?? '').toString(),
      telefone: (json['telefone'] ?? json['Telefone'] ?? '').toString(),
      email: (json['email'] ?? json['Email'] ?? '').toString(),
      totalEventos: _parseInt(json['totalEventos'] ?? json['TotalEventos']),
    );
  }

  static int _parseInt(dynamic val) {
    if (val == null) return 0;
    if (val is int) return val;
    return int.tryParse(val.toString()) ?? 0;
  }
}

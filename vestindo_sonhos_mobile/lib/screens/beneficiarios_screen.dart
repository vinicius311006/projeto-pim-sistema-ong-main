import 'package:flutter/material.dart';
import '../models/beneficiario_model.dart';
import '../services/api_service.dart';

class BeneficiariosScreen extends StatefulWidget {
  const BeneficiariosScreen({super.key});

  @override
  State<BeneficiariosScreen> createState() => _BeneficiariosScreenState();
}

class _BeneficiariosScreenState extends State<BeneficiariosScreen> {
  final _apiService = ApiService();
  List<BeneficiarioModel> _beneficiarios = [];
  List<BeneficiarioModel> _beneficiariosFiltrados = [];
  bool _carregando = true;
  String? _erro;
  final TextEditingController _buscaController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _carregarBeneficiarios();
  }

  @override
  void dispose() {
    _buscaController.dispose();
    super.dispose();
  }

  Future<void> _carregarBeneficiarios() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null) {
        throw Exception('Sessão expirada. Faça login novamente.');
      }
      final dados = await _apiService.getBeneficiarios(token);
      if (mounted) {
        setState(() {
          _beneficiarios = dados;
          _filtrar();
          _carregando = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _erro = e.toString().replaceAll('Exception: ', '');
          _carregando = false;
        });
      }
    }
  }

  void _filtrar() {
    final query = _buscaController.text.toLowerCase().trim();
    if (query.isEmpty) {
      _beneficiariosFiltrados = List.from(_beneficiarios);
    } else {
      _beneficiariosFiltrados = _beneficiarios.where((b) {
        return b.nome.toLowerCase().contains(query) ||
            b.endereco.toLowerCase().contains(query) ||
            (b.observacao?.toLowerCase().contains(query) ?? false);
      }).toList();
    }
  }

  Color _corSituacao(String situacao) {
    switch (situacao.toLowerCase()) {
      case 'ok':
        return const Color(0xFF2E9E5B);
      case 'aguardando doacao':
      case 'aguardando doação':
      case 'aguardando':
        return Colors.orange.shade800;
      case 'bloqueado':
        return const Color(0xFFE25555);
      default:
        return const Color(0xFF7C6FE0);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Beneficiários Atendidos'),
      ),
      body: Column(
        children: [
          // Campo de busca
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: TextField(
              controller: _buscaController,
              decoration: InputDecoration(
                hintText: 'Buscar por nome, endereço ou necessidade...',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: _buscaController.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear),
                        onPressed: () {
                          _buscaController.clear();
                          setState(() {
                            _filtrar();
                          });
                        },
                      )
                    : null,
                filled: true,
                fillColor: Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(color: Colors.grey.shade300),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(color: Colors.grey.shade300),
                ),
              ),
              onChanged: (_) {
                setState(() {
                  _filtrar();
                });
              },
            ),
          ),

          // Lista de Beneficiários
          Expanded(
            child: RefreshIndicator(
              onRefresh: _carregarBeneficiarios,
              child: _buildConteudo(),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildConteudo() {
    if (_carregando) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_erro != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, size: 52, color: Color(0xFFE25555)),
              const SizedBox(height: 12),
              Text(
                _erro!,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 15),
              ),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _carregarBeneficiarios,
                icon: const Icon(Icons.refresh),
                label: const Text('Tentar Novamente'),
              ),
            ],
          ),
        ),
      );
    }

    if (_beneficiariosFiltrados.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.person_search, size: 56, color: Colors.grey.shade400),
              const SizedBox(height: 12),
              Text(
                _buscaController.text.isNotEmpty
                    ? 'Nenhum beneficiário encontrado com esse termo.'
                    : 'Nenhum beneficiário cadastrado no momento.',
                textAlign: TextAlign.center,
                style: TextStyle(fontSize: 16, color: Colors.grey.shade700),
              ),
            ],
          ),
        ),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 4, 16, 20),
      itemCount: _beneficiariosFiltrados.length,
      itemBuilder: (context, index) {
        final b = _beneficiariosFiltrados[index];
        final corStatus = _corSituacao(b.situacao);

        return Card(
          elevation: 1.5,
          margin: const EdgeInsets.symmetric(vertical: 7),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    CircleAvatar(
                      radius: 20,
                      backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.15),
                      child: const Icon(Icons.person, color: Color(0xFF7C6FE0)),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            b.nome,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: Color(0xFF2E2E3A),
                            ),
                          ),
                          const SizedBox(height: 2),
                          Row(
                            children: [
                              const Icon(Icons.phone, size: 14, color: Colors.grey),
                              const SizedBox(width: 4),
                              Text(
                                b.telefone.isNotEmpty ? b.telefone : 'Sem telefone',
                                style: TextStyle(fontSize: 13, color: Colors.grey.shade700),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: corStatus.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(20),
                        border: Border.all(color: corStatus.withValues(alpha: 0.4)),
                      ),
                      child: Text(
                        b.situacao,
                        style: TextStyle(
                          color: corStatus,
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                const Divider(height: 1),
                const SizedBox(height: 10),

                // Endereço
                Row(
                  children: [
                    const Icon(Icons.location_on_outlined, size: 16, color: Color(0xFF7C6FE0)),
                    const SizedBox(width: 6),
                    Expanded(
                      child: Text(
                        b.endereco.isNotEmpty ? b.endereco : 'Endereço não informado',
                        style: const TextStyle(fontSize: 13),
                      ),
                    ),
                  ],
                ),

                if (b.totalDependentes > 0) ...[
                  const SizedBox(height: 6),
                  Row(
                    children: [
                      const Icon(Icons.family_restroom, size: 16, color: Colors.grey),
                      const SizedBox(width: 6),
                      Text(
                        '${b.totalDependentes} dependente(s) associado(s)',
                        style: TextStyle(fontSize: 13, color: Colors.grey.shade700),
                      ),
                    ],
                  ),
                ],

                if (b.observacao != null && b.observacao!.trim().isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF3F4FA),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      'Necessidades: ${b.observacao}',
                      style: const TextStyle(
                        fontSize: 12,
                        fontStyle: FontStyle.italic,
                        color: Color(0xFF6F6F85),
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }
}

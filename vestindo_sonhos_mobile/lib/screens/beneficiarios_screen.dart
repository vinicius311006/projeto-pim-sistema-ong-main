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
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirDialogoSalvar(),
        icon: const Icon(Icons.person_add),
        label: const Text('Novo Beneficiário'),
        backgroundColor: const Color(0xFF7C6FE0),
        foregroundColor: Colors.white,
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
                          const SizedBox(height: 4),
                          Wrap(
                            spacing: 8,
                            runSpacing: 4,
                            crossAxisAlignment: WrapCrossAlignment.center,
                            children: [
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: corStatus.withValues(alpha: 0.12),
                                  borderRadius: BorderRadius.circular(12),
                                  border: Border.all(color: corStatus.withValues(alpha: 0.4)),
                                ),
                                child: Text(
                                  b.situacao,
                                  style: TextStyle(
                                    color: corStatus,
                                    fontWeight: FontWeight.bold,
                                    fontSize: 11,
                                  ),
                                ),
                              ),
                              Row(
                                mainAxisSize: MainAxisSize.min,
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
                        ],
                      ),
                    ),
                    PopupMenuButton<String>(
                      icon: const Icon(Icons.more_vert, size: 22, color: Colors.grey),
                      tooltip: 'Opções',
                      onSelected: (val) {
                        if (val == 'editar') {
                          _abrirDialogoSalvar(beneficiario: b);
                        } else if (val == 'excluir') {
                          _confirmarExclusao(b);
                        }
                      },
                      itemBuilder: (ctx) => [
                        const PopupMenuItem(
                          value: 'editar',
                          child: Row(
                            children: [
                              Icon(Icons.edit_outlined, size: 18, color: Color(0xFF7C6FE0)),
                              SizedBox(width: 8),
                              Text('Editar'),
                            ],
                          ),
                        ),
                        const PopupMenuItem(
                          value: 'excluir',
                          child: Row(
                            children: [
                              Icon(Icons.delete_outline, size: 18, color: Color(0xFFE25555)),
                              SizedBox(width: 8),
                              Text('Excluir', style: TextStyle(color: Color(0xFFE25555))),
                            ],
                          ),
                        ),
                      ],
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

  Future<void> _abrirDialogoSalvar({BeneficiarioModel? beneficiario}) async {
    final ehEdicao = beneficiario != null;
    final formKey = GlobalKey<FormState>();
    final nomeCtrl = TextEditingController(text: beneficiario?.nome ?? '');
    final telCtrl = TextEditingController(text: beneficiario?.telefone ?? '');
    final endCtrl = TextEditingController(text: beneficiario?.endereco ?? '');
    final obsCtrl = TextEditingController(text: beneficiario?.observacao ?? '');
    String situacao = beneficiario?.situacao ?? 'Ok';
    bool salvando = false;

    // Normaliza situacao para dropdown
    if (!['Ok', 'Aguardando', 'Bloqueado'].contains(situacao)) {
      situacao = 'Ok';
    }

    await showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (dialogCtx, setDialogState) {
            return AlertDialog(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
              title: Row(
                children: [
                  Icon(
                    ehEdicao ? Icons.edit_outlined : Icons.person_add_outlined,
                    color: const Color(0xFF7C6FE0),
                  ),
                  const SizedBox(width: 8),
                  Text(
                    ehEdicao ? 'Editar Beneficiário' : 'Novo Beneficiário',
                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              content: SizedBox(
                width: double.maxFinite,
                child: Form(
                  key: formKey,
                  child: SingleChildScrollView(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        TextFormField(
                          controller: nomeCtrl,
                          decoration: const InputDecoration(
                            labelText: 'Nome Completo *',
                            prefixIcon: Icon(Icons.person_outline),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 100,
                          validator: (v) {
                            if (v == null || v.trim().isEmpty) {
                              return 'Informe o nome do beneficiário';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: telCtrl,
                          keyboardType: TextInputType.phone,
                          decoration: const InputDecoration(
                            labelText: 'Telefone (máx. 11 dígitos)',
                            prefixIcon: Icon(Icons.phone_outlined),
                            hintText: '11999998888',
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 11,
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: endCtrl,
                          decoration: const InputDecoration(
                            labelText: 'Endereço',
                            prefixIcon: Icon(Icons.location_on_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 200,
                        ),
                        const SizedBox(height: 12),
                        DropdownButtonFormField<String>(
                          initialValue: situacao,
                          isExpanded: true,
                          decoration: const InputDecoration(
                            labelText: 'Situação',
                            prefixIcon: Icon(Icons.flag_outlined),
                            border: OutlineInputBorder(),
                          ),
                          items: const [
                            DropdownMenuItem(value: 'Ok', child: Text('Ok / Ativo')),
                            DropdownMenuItem(value: 'Aguardando', child: Text('Aguardando Doação')),
                            DropdownMenuItem(value: 'Bloqueado', child: Text('Bloqueado')),
                          ],
                          onChanged: (val) {
                            if (val != null) {
                              setDialogState(() => situacao = val);
                            }
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: obsCtrl,
                          maxLines: 3,
                          decoration: const InputDecoration(
                            labelText: 'Necessidades / Observações',
                            prefixIcon: Icon(Icons.notes_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 500,
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: salvando ? null : () => Navigator.pop(dialogCtx),
                  child: const Text('Cancelar'),
                ),
                ElevatedButton(
                  onPressed: salvando
                      ? null
                      : () async {
                          if (!formKey.currentState!.validate()) return;
                          setDialogState(() => salvando = true);

                          final dados = {
                            'nome': nomeCtrl.text.trim(),
                            'telefone': telCtrl.text.trim(),
                            'endereco': endCtrl.text.trim(),
                            'situacao': situacao,
                            'observacao': obsCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            if (ehEdicao) {
                              await _apiService.editarBeneficiario(token, beneficiario.id, dados);
                            } else {
                              await _apiService.criarBeneficiario(token, dados);
                            }

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(
                                  content: Text(
                                    ehEdicao
                                        ? 'Beneficiário atualizado com sucesso!'
                                        : 'Beneficiário cadastrado com sucesso!',
                                  ),
                                  backgroundColor: const Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarBeneficiarios();
                            }
                          } catch (e) {
                            setDialogState(() => salvando = false);
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(
                                  content: Text(e.toString().replaceAll('Exception: ', '')),
                                  backgroundColor: const Color(0xFFE25555),
                                ),
                              );
                            }
                          }
                        },
                  child: salvando
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : Text(ehEdicao ? 'Salvar' : 'Cadastrar'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  Future<void> _confirmarExclusao(BeneficiarioModel b) async {
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Color(0xFFE25555)),
            SizedBox(width: 8),
            Text('Confirmar Exclusão'),
          ],
        ),
        content: Text('Deseja realmente excluir o beneficiário "${b.nome}"? Esta ação não pode ser desfeita.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFFE25555)),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Excluir', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmar != true) return;

    try {
      final token = await _apiService.getToken();
      if (token == null) throw Exception('Não autenticado.');

      await _apiService.excluirBeneficiario(token, b.id);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Beneficiário excluído com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
          ),
        );
        _carregarBeneficiarios();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceAll('Exception: ', '')),
            backgroundColor: const Color(0xFFE25555),
          ),
        );
      }
    }
  }
}

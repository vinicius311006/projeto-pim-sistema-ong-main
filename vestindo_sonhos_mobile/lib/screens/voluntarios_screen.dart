import 'package:flutter/material.dart';
import '../models/voluntario_model.dart';
import '../services/api_service.dart';

/// Tela de Gestão de Voluntários da ONG Vestindo Sonhos.
/// Acessível tanto para Colaboradores quanto Administradores (listagem, cadastro, edição e exclusão).
class VoluntariosScreen extends StatefulWidget {
  const VoluntariosScreen({super.key});

  @override
  State<VoluntariosScreen> createState() => _VoluntariosScreenState();
}

class _VoluntariosScreenState extends State<VoluntariosScreen> {
  final _apiService = ApiService();
  List<VoluntarioModel> _voluntarios = [];
  List<VoluntarioModel> _voluntariosFiltrados = [];
  bool _carregando = true;
  String? _erro;
  final TextEditingController _buscaController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _carregarVoluntarios();
  }

  @override
  void dispose() {
    _buscaController.dispose();
    super.dispose();
  }

  Future<void> _carregarVoluntarios() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null) {
        throw Exception('Sessão expirada. Faça login novamente.');
      }
      final dados = await _apiService.getVoluntarios(token);
      if (mounted) {
        setState(() {
          _voluntarios = dados;
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
      _voluntariosFiltrados = List.from(_voluntarios);
    } else {
      _voluntariosFiltrados = _voluntarios.where((v) {
        return v.nome.toLowerCase().contains(query) ||
            v.email.toLowerCase().contains(query) ||
            v.telefone.toLowerCase().contains(query) ||
            v.cpf.toLowerCase().contains(query);
      }).toList();
    }
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Voluntários da ONG'),
      ),
      body: SafeArea(
        child: Column(
          children: [
            // Campo de busca
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
              child: TextField(
                controller: _buscaController,
                decoration: InputDecoration(
                  hintText: 'Buscar voluntário por nome, e-mail ou telefone...',
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
                  fillColor: isDark ? const Color(0xFF1F2232) : Colors.white,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide(color: isDark ? const Color(0xFF2D324A) : Colors.grey.shade300),
                  ),
                  enabledBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide(color: isDark ? const Color(0xFF2D324A) : Colors.grey.shade300),
                  ),
                ),
                onChanged: (_) {
                  setState(() {
                    _filtrar();
                  });
                },
              ),
            ),

            // Lista de Voluntários
            Expanded(
              child: RefreshIndicator(
                onRefresh: _carregarVoluntarios,
                child: _buildConteudo(isDark),
              ),
            ),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirDialogoSalvar(),
        icon: const Icon(Icons.person_add_alt_1_rounded),
        label: const Text('Novo Voluntário'),
        backgroundColor: const Color(0xFF7C6FE0),
        foregroundColor: Colors.white,
      ),
    );
  }

  Widget _buildConteudo(bool isDark) {
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
                onPressed: _carregarVoluntarios,
                icon: const Icon(Icons.refresh),
                label: const Text('Tentar Novamente'),
              ),
            ],
          ),
        ),
      );
    }

    if (_voluntariosFiltrados.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.volunteer_activism_outlined, size: 56, color: Colors.grey.shade400),
              const SizedBox(height: 12),
              Text(
                _buscaController.text.isNotEmpty
                    ? 'Nenhum voluntário encontrado com esse termo.'
                    : 'Nenhum voluntário cadastrado no momento.',
                textAlign: TextAlign.center,
                style: TextStyle(fontSize: 16, color: Colors.grey.shade600),
              ),
            ],
          ),
        ),
      );
    }

    final textPrimary = isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A);
    final textSecondary = isDark ? const Color(0xFF9CA3AF) : Colors.grey.shade700;

    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 4, 16, 80),
      itemCount: _voluntariosFiltrados.length,
      itemBuilder: (context, index) {
        final v = _voluntariosFiltrados[index];

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
                      backgroundColor: const Color(0xFF00B4D8).withValues(alpha: 0.15),
                      child: const Icon(Icons.volunteer_activism_rounded, color: Color(0xFF00B4D8), size: 20),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            v.nome,
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: textPrimary,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Wrap(
                            spacing: 10,
                            runSpacing: 4,
                            crossAxisAlignment: WrapCrossAlignment.center,
                            children: [
                              if (v.telefone.isNotEmpty)
                                Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    const Icon(Icons.phone_outlined, size: 14, color: Colors.grey),
                                    const SizedBox(width: 4),
                                    Text(v.telefone, style: TextStyle(fontSize: 13, color: textSecondary)),
                                  ],
                                ),
                              if (v.cpf.isNotEmpty)
                                Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    const Icon(Icons.badge_outlined, size: 14, color: Colors.grey),
                                    const SizedBox(width: 4),
                                    Text('CPF: ${v.cpf}', style: TextStyle(fontSize: 12, color: textSecondary)),
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
                          _abrirDialogoSalvar(voluntario: v);
                        } else if (val == 'excluir') {
                          _confirmarExclusao(v);
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
                if (v.email.isNotEmpty || v.endereco.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  const Divider(height: 1),
                  const SizedBox(height: 8),
                  if (v.email.isNotEmpty)
                    Row(
                      children: [
                        const Icon(Icons.email_outlined, size: 15, color: Color(0xFF7C6FE0)),
                        const SizedBox(width: 6),
                        Expanded(
                          child: Text(
                            v.email,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(fontSize: 13, color: textSecondary),
                          ),
                        ),
                      ],
                    ),
                  if (v.endereco.isNotEmpty) ...[
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        const Icon(Icons.location_on_outlined, size: 15, color: Color(0xFF7C6FE0)),
                        const SizedBox(width: 6),
                        Expanded(
                          child: Text(
                            v.endereco,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(fontSize: 13, color: textSecondary),
                          ),
                        ),
                      ],
                    ),
                  ],
                ],
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _abrirDialogoSalvar({VoluntarioModel? voluntario}) async {
    final ehEdicao = voluntario != null;
    final formKey = GlobalKey<FormState>();
    final nomeCtrl = TextEditingController(text: voluntario?.nome ?? '');
    final cpfCtrl = TextEditingController(text: voluntario?.cpf ?? '');
    final telCtrl = TextEditingController(text: voluntario?.telefone ?? '');
    final emailCtrl = TextEditingController(text: voluntario?.email ?? '');
    final endCtrl = TextEditingController(text: voluntario?.endereco ?? '');
    bool salvando = false;

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
                    ehEdicao ? Icons.edit_outlined : Icons.person_add_alt_1_rounded,
                    color: const Color(0xFF7C6FE0),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      ehEdicao ? 'Editar Voluntário' : 'Novo Voluntário',
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                    ),
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
                          maxLength: 60,
                          validator: (v) {
                            if (v == null || v.trim().isEmpty) {
                              return 'Informe o nome do voluntário';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: cpfCtrl,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'CPF (somente números)',
                            prefixIcon: Icon(Icons.badge_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 11,
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: telCtrl,
                          keyboardType: TextInputType.phone,
                          decoration: const InputDecoration(
                            labelText: 'Telefone / WhatsApp',
                            prefixIcon: Icon(Icons.phone_outlined),
                            hintText: '11999998888',
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 11,
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: emailCtrl,
                          keyboardType: TextInputType.emailAddress,
                          decoration: const InputDecoration(
                            labelText: 'E-mail',
                            prefixIcon: Icon(Icons.email_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 40,
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: endCtrl,
                          decoration: const InputDecoration(
                            labelText: 'Endereço',
                            prefixIcon: Icon(Icons.location_on_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 50,
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
                            'cpf': cpfCtrl.text.trim(),
                            'telefone': telCtrl.text.trim(),
                            'email': emailCtrl.text.trim(),
                            'endereco': endCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            if (ehEdicao) {
                              await _apiService.editarVoluntario(token, voluntario.id, dados);
                            } else {
                              await _apiService.criarVoluntario(token, dados);
                            }

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(
                                  content: Text(
                                    ehEdicao
                                        ? 'Voluntário atualizado com sucesso!'
                                        : 'Voluntário cadastrado com sucesso!',
                                  ),
                                  backgroundColor: const Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarVoluntarios();
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

  Future<void> _confirmarExclusao(VoluntarioModel v) async {
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Color(0xFFE25555)),
            SizedBox(width: 8),
            Expanded(child: Text('Confirmar Exclusão')),
          ],
        ),
        content: Text('Deseja realmente excluir o voluntário "${v.nome}"? Esta ação não pode ser desfeita.'),
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

      await _apiService.excluirVoluntario(token, v.id);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Voluntário excluído com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
          ),
        );
        _carregarVoluntarios();
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

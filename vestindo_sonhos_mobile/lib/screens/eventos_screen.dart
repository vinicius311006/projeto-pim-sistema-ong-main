import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/evento_model.dart';
import '../services/api_service.dart';

/// Tela de Listagem de Eventos da ONG Vestindo Sonhos.
/// Consome o endpoint GET /api/eventos protegido por JWT e permite pull-to-refresh com RefreshIndicator.
class EventosScreen extends StatefulWidget {
  const EventosScreen({super.key});

  @override
  State<EventosScreen> createState() => _EventosScreenState();
}

class _EventosScreenState extends State<EventosScreen> {
  final _apiService = ApiService();

  List<EventoModel> _eventos = [];
  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _carregarEventos();
  }

  /// Busca os eventos ativos a partir da API REST.
  Future<void> _carregarEventos() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final token = await _apiService.getToken();

      if (token == null || token.isEmpty) {
        if (!mounted) return;
        Navigator.pushReplacementNamed(context, '/login');
        return;
      }

      final eventosRecebidos = await _apiService.getEventos(token);

      if (!mounted) return;
      setState(() {
        _eventos = eventosRecebidos;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Eventos Ativos'),
        centerTitle: true,
      ),
      body: RefreshIndicator(
        onRefresh: _carregarEventos,
        color: theme.colorScheme.primary,
        child: _buildCorpo(theme),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirDialogoSalvar(),
        icon: const Icon(Icons.add_circle_outline),
        label: const Text('Novo Evento'),
        backgroundColor: const Color(0xFF7C6FE0),
        foregroundColor: Colors.white,
      ),
    );
  }

  Widget _buildCorpo(ThemeData theme) {
    // 1. Estado de carregamento
    if (_isLoading) {
      return const Center(
        child: CircularProgressIndicator(),
      );
    }

    // 2. Estado de erro na requisição
    if (_errorMessage != null) {
      return Center(
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.error_outline_rounded,
                size: 64,
                color: Colors.red.shade400,
              ),
              const SizedBox(height: 16),
              Text(
                'Falha ao carregar eventos',
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                _errorMessage!,
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey.shade600),
              ),
              const SizedBox(height: 20),
              ElevatedButton.icon(
                onPressed: _carregarEventos,
                icon: const Icon(Icons.refresh),
                label: const Text('Tentar Novamente'),
              ),
            ],
          ),
        ),
      );
    }

    // 3. Lista vazia
    if (_eventos.isEmpty) {
      return Center(
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.event_busy_outlined,
                size: 72,
                color: Colors.grey.shade400,
              ),
              const SizedBox(height: 16),
              Text(
                'Nenhum evento ativo',
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'No momento não existem eventos cadastrados com data futura. Puxe a tela para atualizar.',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey.shade600),
              ),
            ],
          ),
        ),
      );
    }

    // 4. Lista preenchida com ListView.builder
    return ListView.builder(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
      itemCount: _eventos.length,
      itemBuilder: (context, index) {
        final evento = _eventos[index];
        return _buildCardEvento(evento, theme);
      },
    );
  }

  /// Constrói o Card visual de cada Evento
  Widget _buildCardEvento(EventoModel evento, ThemeData theme) {
    return Card(
      margin: const EdgeInsets.only(bottom: 14.0),
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(14),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Cabeçalho do Card: Nome e Tag de Voluntários
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Text(
                    evento.nome,
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: theme.colorScheme.primary.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.group_outlined,
                            size: 16,
                            color: theme.colorScheme.primary,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '${evento.totalVoluntarios}',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                              color: theme.colorScheme.primary,
                            ),
                          ),
                        ],
                      ),
                    ),
                    PopupMenuButton<String>(
                      icon: const Icon(Icons.more_vert, size: 20, color: Colors.grey),
                      tooltip: 'Opções',
                      onSelected: (val) {
                        if (val == 'editar') {
                          _abrirDialogoSalvar(evento: evento);
                        } else if (val == 'excluir') {
                          _confirmarExclusao(evento);
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
              ],
            ),
            const SizedBox(height: 12),

            // Data / Período
            Row(
              children: [
                Icon(Icons.calendar_today_outlined, size: 18, color: Colors.grey.shade500),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    evento.periodoFormatado,
                    style: TextStyle(
                      fontSize: 14,
                      color: theme.brightness == Brightness.dark
                          ? const Color(0xFFE5E7EB)
                          : Colors.grey.shade800,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),

            // Local
            if (evento.local != null && evento.local!.isNotEmpty) ...[
              Row(
                children: [
                  Icon(Icons.location_on_outlined, size: 18, color: Colors.grey.shade500),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      evento.local!,
                      style: TextStyle(
                        fontSize: 14,
                        color: theme.brightness == Brightness.dark
                            ? const Color(0xFFE5E7EB)
                            : Colors.grey.shade800,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
            ],

            // Responsável
            Row(
              children: [
                Icon(Icons.person_outline, size: 18, color: Colors.grey.shade500),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Responsável: ${evento.responsavelNome}',
                    style: TextStyle(
                      fontSize: 13,
                      color: theme.brightness == Brightness.dark
                          ? const Color(0xFF9CA3AF)
                          : Colors.grey.shade700,
                    ),
                  ),
                ),
              ],
            ),

            // Observações
            if (evento.observacao != null && evento.observacao!.isNotEmpty) ...[
              const Divider(height: 20),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Icon(Icons.info_outline, size: 16, color: Colors.grey.shade500),
                  const SizedBox(width: 6),
                  Expanded(
                    child: Text(
                      evento.observacao!,
                      style: TextStyle(
                        fontSize: 13,
                        color: Colors.grey.shade600,
                        fontStyle: FontStyle.italic,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }

  Future<void> _abrirDialogoSalvar({EventoModel? evento}) async {
    final ehEdicao = evento != null;
    final formKey = GlobalKey<FormState>();
    final nomeCtrl = TextEditingController(text: evento?.nome ?? '');
    DateTime dataInicio = evento?.dataInicio ?? DateTime.now().add(const Duration(days: 1));
    DateTime? dataFim = evento?.dataFim;
    final localCtrl = TextEditingController(text: evento?.local ?? '');
    final obsCtrl = TextEditingController(text: evento?.observacao ?? '');
    bool salvando = false;

    final dateFormat = DateFormat('dd/MM/yyyy HH:mm');

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
                    ehEdicao ? Icons.edit_calendar_outlined : Icons.add_circle_outline,
                    color: const Color(0xFF7C6FE0),
                  ),
                  const SizedBox(width: 8),
                  Text(
                    ehEdicao ? 'Editar Evento' : 'Novo Evento',
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
                            labelText: 'Nome do Evento *',
                            prefixIcon: Icon(Icons.event_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 100,
                          validator: (v) {
                            if (v == null || v.trim().isEmpty) {
                              return 'Informe o nome do evento';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),

                        // Seleção de Data/Hora de Início
                        InkWell(
                          onTap: () async {
                            final pickedDate = await showDatePicker(
                              context: dialogCtx,
                              initialDate: dataInicio,
                              firstDate: DateTime(2020),
                              lastDate: DateTime(2040),
                            );
                            if (pickedDate != null && dialogCtx.mounted) {
                              final pickedTime = await showTimePicker(
                                context: dialogCtx,
                                initialTime: TimeOfDay.fromDateTime(dataInicio),
                              );
                              if (pickedTime != null) {
                                setDialogState(() {
                                  dataInicio = DateTime(
                                    pickedDate.year,
                                    pickedDate.month,
                                    pickedDate.day,
                                    pickedTime.hour,
                                    pickedTime.minute,
                                  );
                                });
                              } else {
                                setDialogState(() {
                                  dataInicio = DateTime(
                                    pickedDate.year,
                                    pickedDate.month,
                                    pickedDate.day,
                                    dataInicio.hour,
                                    dataInicio.minute,
                                  );
                                });
                              }
                            }
                          },
                          child: InputDecorator(
                            decoration: const InputDecoration(
                              labelText: 'Data e Hora de Início *',
                              prefixIcon: Icon(Icons.calendar_today_outlined),
                              border: OutlineInputBorder(),
                            ),
                            child: Text(
                              dateFormat.format(dataInicio),
                              style: const TextStyle(fontSize: 15),
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        // Seleção de Data/Hora de Término (Opcional)
                        InkWell(
                          onTap: () async {
                            final pickedDate = await showDatePicker(
                              context: dialogCtx,
                              initialDate: dataFim ?? dataInicio.add(const Duration(hours: 3)),
                              firstDate: DateTime(2020),
                              lastDate: DateTime(2040),
                            );
                            if (pickedDate != null && dialogCtx.mounted) {
                              final pickedTime = await showTimePicker(
                                context: dialogCtx,
                                initialTime: TimeOfDay.fromDateTime(dataFim ?? dataInicio),
                              );
                              if (pickedTime != null) {
                                setDialogState(() {
                                  dataFim = DateTime(
                                    pickedDate.year,
                                    pickedDate.month,
                                    pickedDate.day,
                                    pickedTime.hour,
                                    pickedTime.minute,
                                  );
                                });
                              } else {
                                setDialogState(() {
                                  dataFim = pickedDate;
                                });
                              }
                            }
                          },
                          child: InputDecorator(
                            decoration: InputDecoration(
                              labelText: 'Data e Hora de Término (Opcional)',
                              prefixIcon: const Icon(Icons.event_available_outlined),
                              suffixIcon: dataFim != null
                                  ? IconButton(
                                      icon: const Icon(Icons.clear, size: 18),
                                      onPressed: () {
                                        setDialogState(() => dataFim = null);
                                      },
                                    )
                                  : null,
                              border: const OutlineInputBorder(),
                            ),
                            child: Text(
                              dataFim != null ? dateFormat.format(dataFim!) : 'Não definido',
                              style: TextStyle(
                                fontSize: 15,
                                color: dataFim != null ? Colors.black87 : Colors.grey.shade600,
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        TextFormField(
                          controller: localCtrl,
                          decoration: const InputDecoration(
                            labelText: 'Local do Evento',
                            prefixIcon: Icon(Icons.location_on_outlined),
                            border: OutlineInputBorder(),
                          ),
                          maxLength: 150,
                        ),
                        const SizedBox(height: 12),

                        TextFormField(
                          controller: obsCtrl,
                          maxLines: 3,
                          decoration: const InputDecoration(
                            labelText: 'Descrição / Informações',
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
                            'dataInicio': dataInicio.toIso8601String(),
                            'dataFim': dataFim?.toIso8601String(),
                            'local': localCtrl.text.trim(),
                            'observacao': obsCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            if (ehEdicao) {
                              await _apiService.editarEvento(token, evento.id, dados);
                            } else {
                              await _apiService.criarEvento(token, dados);
                            }

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                SnackBar(
                                  content: Text(
                                    ehEdicao
                                        ? 'Evento atualizado com sucesso!'
                                        : 'Evento cadastrado com sucesso!',
                                  ),
                                  backgroundColor: const Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarEventos();
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

  Future<void> _confirmarExclusao(EventoModel evento) async {
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
        content: Text('Deseja realmente excluir o evento "${evento.nome}"? Esta ação não pode ser desfeita.'),
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

      await _apiService.excluirEvento(token, evento.id);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Evento excluído com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
          ),
        );
        _carregarEventos();
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

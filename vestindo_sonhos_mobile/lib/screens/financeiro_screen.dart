import 'package:flutter/material.dart';
import '../models/financeiro_model.dart';
import '../services/api_service.dart';

class FinanceiroScreen extends StatefulWidget {
  const FinanceiroScreen({super.key});

  @override
  State<FinanceiroScreen> createState() => _FinanceiroScreenState();
}

class _FinanceiroScreenState extends State<FinanceiroScreen> {
  final _apiService = ApiService();
  FinanceiroModel? _financeiro;
  bool _carregando = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregarFinanceiro();
  }

  Future<void> _carregarFinanceiro() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null) {
        throw Exception('Sessão expirada. Faça login novamente.');
      }
      final dados = await _apiService.getFinanceiro(token);
      if (mounted) {
        setState(() {
          _financeiro = dados;
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

  String _formatarMoeda(double valor) {
    return 'R\$ ${valor.toStringAsFixed(2).replaceAll('.', ',')}';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Gestão Financeira & Doações'),
      ),
      body: RefreshIndicator(
        onRefresh: _carregarFinanceiro,
        child: _buildConteudo(),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _abrirDialogoNovaTransacao,
        icon: const Icon(Icons.add),
        label: const Text('Novo Lançamento'),
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
              Text(_erro!, textAlign: TextAlign.center, style: const TextStyle(fontSize: 15)),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _carregarFinanceiro,
                icon: const Icon(Icons.refresh),
                label: const Text('Tentar Novamente'),
              ),
            ],
          ),
        ),
      );
    }

    final f = _financeiro!;

    return SingleChildScrollView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Card de Saldo Total Geral
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF7C6FE0), Color(0xFF9B8DE3)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: const Color(0xFF7C6FE0).withValues(alpha: 0.3),
                  blurRadius: 10,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Saldo Total Disponível',
                  style: TextStyle(color: Colors.white70, fontSize: 14),
                ),
                const SizedBox(height: 6),
                Text(
                  _formatarMoeda(f.saldoTotalGeral),
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 16),
                const Divider(color: Colors.white24, height: 1),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: Row(
                        children: [
                          const CircleAvatar(
                            radius: 14,
                            backgroundColor: Colors.white24,
                            child: Icon(Icons.arrow_downward, color: Colors.greenAccent, size: 16),
                          ),
                          const SizedBox(width: 8),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text('Entradas', style: TextStyle(color: Colors.white70, fontSize: 11)),
                              Text(
                                _formatarMoeda(f.totalEntradas),
                                style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.bold),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    Expanded(
                      child: Row(
                        children: [
                          const CircleAvatar(
                            radius: 14,
                            backgroundColor: Colors.white24,
                            child: Icon(Icons.arrow_upward, color: Colors.redAccent, size: 16),
                          ),
                          const SizedBox(width: 8),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text('Saídas', style: TextStyle(color: Colors.white70, fontSize: 11)),
                              Text(
                                _formatarMoeda(f.totalSaidas),
                                style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.bold),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Seção de Carteiras
          const Text(
            'Carteiras & Contas',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Color(0xFF2E2E3A),
            ),
          ),
          const SizedBox(height: 12),

          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: Row(
              children: f.carteiras.map((c) {
                return Container(
                  width: 170,
                  margin: const EdgeInsets.only(right: 12),
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.grey.shade200),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.03),
                        blurRadius: 4,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      CircleAvatar(
                        radius: 16,
                        backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
                        child: const Icon(Icons.account_balance_wallet, size: 18, color: Color(0xFF7C6FE0)),
                      ),
                      const SizedBox(height: 10),
                      Text(
                        c.nome,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(fontSize: 13, color: Color(0xFF6F6F85)),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        _formatarMoeda(c.saldo),
                        style: const TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF2E2E3A),
                        ),
                      ),
                    ],
                  ),
                );
              }).toList(),
            ),
          ),
          const SizedBox(height: 24),

          // Seção de Transações Recentes
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Últimas Movimentações',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF2E2E3A),
                ),
              ),
              Text(
                '${f.ultimasTransacoes.length} registros',
                style: const TextStyle(color: Color(0xFF6F6F85), fontSize: 13),
              ),
            ],
          ),
          const SizedBox(height: 10),

          if (f.ultimasTransacoes.isEmpty)
            Center(
              child: Padding(
                padding: const EdgeInsets.all(20.0),
                child: Text('Nenhuma movimentação financeira recente.', style: TextStyle(color: Colors.grey.shade600)),
              ),
            )
          else
            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: f.ultimasTransacoes.length,
              itemBuilder: (context, index) {
                final t = f.ultimasTransacoes[index];
                final isEntrada = t.razao.toLowerCase().contains('entrada');
                final cor = isEntrada ? const Color(0xFF2E9E5B) : const Color(0xFFE25555);

                return Card(
                  margin: const EdgeInsets.symmetric(vertical: 6),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  elevation: 1,
                  child: ListTile(
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
                    leading: CircleAvatar(
                      backgroundColor: cor.withValues(alpha: 0.12),
                      child: Icon(
                        isEntrada ? Icons.arrow_downward : Icons.arrow_upward,
                        color: cor,
                        size: 20,
                      ),
                    ),
                    title: Text(
                      t.categoria,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                    ),
                    subtitle: Text(
                      '${t.carteira} • ${t.observacao ?? t.razao}',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 12),
                    ),
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          '${isEntrada ? '+' : '-'} ${_formatarMoeda(t.valor)}',
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            color: cor,
                            fontSize: 14,
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete_outline, size: 20, color: Color(0xFFE25555)),
                          tooltip: 'Excluir Lançamento',
                          onPressed: () => _confirmarExclusaoTransacao(t),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
        ],
      ),
    );
  }

  Future<void> _abrirDialogoNovaTransacao() async {
    final f = _financeiro;
    if (f == null || f.carteiras.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Nenhuma carteira disponível para registrar lançamentos.'),
          backgroundColor: Color(0xFFE25555),
        ),
      );
      return;
    }

    final formKey = GlobalKey<FormState>();
    int carteiraId = f.carteiras.first.id;
    String tipo = 'Entrada';
    final valorCtrl = TextEditingController();
    final obsCtrl = TextEditingController();
    bool salvando = false;

    await showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (dialogCtx, setDialogState) {
            return AlertDialog(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
              title: const Row(
                children: [
                  Icon(Icons.monetization_on_outlined, color: Color(0xFF7C6FE0)),
                  SizedBox(width: 8),
                  Text('Novo Lançamento', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
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
                        // Tipo (Entrada ou Saída)
                        DropdownButtonFormField<String>(
                          initialValue: tipo,
                          isExpanded: true,
                          decoration: const InputDecoration(
                            labelText: 'Tipo de Lançamento *',
                            prefixIcon: Icon(Icons.compare_arrows),
                            border: OutlineInputBorder(),
                          ),
                          items: const [
                            DropdownMenuItem(value: 'Entrada', child: Text('Entrada (Doação / Receita)')),
                            DropdownMenuItem(value: 'Saída', child: Text('Saída (Despesa / Pagamento)')),
                          ],
                          onChanged: (val) {
                            if (val != null) {
                              setDialogState(() => tipo = val);
                            }
                          },
                        ),
                        const SizedBox(height: 12),

                        // Seleção de Carteira
                        DropdownButtonFormField<int>(
                          initialValue: carteiraId,
                          isExpanded: true,
                          decoration: const InputDecoration(
                            labelText: 'Carteira / Conta *',
                            prefixIcon: Icon(Icons.account_balance_wallet_outlined),
                            border: OutlineInputBorder(),
                          ),
                          items: f.carteiras.map((c) {
                            return DropdownMenuItem<int>(
                              value: c.id,
                              child: Text('${c.nome} (Saldo: ${_formatarMoeda(c.saldo)})'),
                            );
                          }).toList(),
                          onChanged: (val) {
                            if (val != null) {
                              setDialogState(() => carteiraId = val);
                            }
                          },
                        ),
                        const SizedBox(height: 12),

                        // Valor
                        TextFormField(
                          controller: valorCtrl,
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          decoration: const InputDecoration(
                            labelText: 'Valor (R\$) *',
                            hintText: 'Ex: 150,00 ou 150.00',
                            prefixIcon: Icon(Icons.attach_money),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) {
                            if (v == null || v.trim().isEmpty) {
                              return 'Informe o valor da transação';
                            }
                            final normalizado = v.trim().replaceAll(',', '.');
                            final num = double.tryParse(normalizado);
                            if (num == null || num <= 0) {
                              return 'Informe um valor numérico maior que zero';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),

                        // Observação / Motivo
                        TextFormField(
                          controller: obsCtrl,
                          maxLines: 2,
                          maxLength: 255,
                          decoration: const InputDecoration(
                            labelText: 'Descrição / Motivo (Opcional)',
                            prefixIcon: Icon(Icons.notes_outlined),
                            border: OutlineInputBorder(),
                          ),
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

                          final normalizado = valorCtrl.text.trim().replaceAll(',', '.');
                          final valorDouble = double.parse(normalizado);

                          final dados = {
                            'carteiraId': carteiraId,
                            'razao': tipo,
                            'valor': valorDouble,
                            'observacao': obsCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            await _apiService.criarTransacaoFinanceira(token, dados);

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text('Lançamento registrado com sucesso!'),
                                  backgroundColor: Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarFinanceiro();
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
                      : const Text('Salvar'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  Future<void> _confirmarExclusaoTransacao(TransacaoModel t) async {
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
        content: Text(
          'Deseja excluir este lançamento de ${_formatarMoeda(t.valor)} (${t.carteira})?\n'
          'O saldo da carteira será devidamente estornado.',
        ),
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

      await _apiService.excluirTransacaoFinanceira(token, t.id);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Lançamento excluído e saldo estornado com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
          ),
        );
        _carregarFinanceiro();
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

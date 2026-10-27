import 'package:flutter/material.dart';
import '../models/inventario_model.dart';
import '../services/api_service.dart';

class InventarioScreen extends StatefulWidget {
  const InventarioScreen({super.key});

  @override
  State<InventarioScreen> createState() => _InventarioScreenState();
}

class _InventarioScreenState extends State<InventarioScreen> {
  final _apiService = ApiService();
  List<InventarioModel> _itens = [];
  List<InventarioModel> _itensFiltrados = [];
  bool _carregando = true;
  String? _erro;
  String _categoriaSelecionada = 'Todas';
  final TextEditingController _buscaController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _carregarInventario();
  }

  @override
  void dispose() {
    _buscaController.dispose();
    super.dispose();
  }

  Future<void> _carregarInventario() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null) {
        throw Exception('Sessão expirada. Faça login novamente.');
      }
      final dados = await _apiService.getInventario(token);
      if (mounted) {
        setState(() {
          _itens = dados;
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
    _itensFiltrados = _itens.where((item) {
      final bateCategoria = _categoriaSelecionada == 'Todas' ||
          item.categoria.toLowerCase() == _categoriaSelecionada.toLowerCase();
      final bateBusca = query.isEmpty ||
          item.nomeProduto.toLowerCase().contains(query) ||
          item.descricao.toLowerCase().contains(query) ||
          item.cor.toLowerCase().contains(query);
      return bateCategoria && bateBusca;
    }).toList();
  }

  List<String> get _categoriasDisponiveis {
    final setCat = <String>{'Todas'};
    for (final i in _itens) {
      if (i.categoria.isNotEmpty) setCat.add(i.categoria);
    }
    return setCat.toList();
  }

  int get _totalEstoqueGeral {
    return _itens.fold(0, (sum, i) => sum + i.quantidadeEmEstoque);
  }

  Color _corEstoque(int qtd) {
    if (qtd <= 0) return const Color(0xFFE25555);
    if (qtd <= 10) return Colors.orange.shade800;
    return const Color(0xFF2E9E5B);
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final textPrimary = isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Inventário & Estoque'),
      ),
      body: Column(
        children: [
          // Card de Resumo de Estoque
          Container(
            margin: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: const Color(0xFF7C6FE0).withValues(alpha: 0.25)),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _buildInfoResumo('Produtos Cadastrados', '${_itens.length} itens', Icons.category_outlined, isDark),
                Container(height: 30, width: 1, color: isDark ? const Color(0xFF2D324A) : Colors.grey.shade300),
                _buildInfoResumo('Total em Estoque', '$_totalEstoqueGeral un', Icons.inventory_2_outlined, isDark),
              ],
            ),
          ),

          // Campo de busca
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: TextField(
              controller: _buscaController,
              decoration: InputDecoration(
                hintText: 'Buscar produto por nome ou cor...',
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

          // Chips de Categorias
          if (_categoriasDisponiveis.length > 1)
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: Row(
                children: _categoriasDisponiveis.map((cat) {
                  final selecionada = _categoriaSelecionada == cat;
                  return Padding(
                    padding: const EdgeInsets.only(right: 8.0),
                    child: FilterChip(
                      selected: selecionada,
                      label: Text(cat),
                      selectedColor: const Color(0xFF7C6FE0).withValues(alpha: 0.2),
                      checkmarkColor: const Color(0xFF7C6FE0),
                      labelStyle: TextStyle(
                        color: selecionada ? const Color(0xFF7C6FE0) : textPrimary,
                        fontWeight: selecionada ? FontWeight.bold : FontWeight.normal,
                      ),
                      onSelected: (val) {
                        setState(() {
                          _categoriaSelecionada = cat;
                          _filtrar();
                        });
                      },
                    ),
                  );
                }).toList(),
              ),
            ),

          // Lista de Produtos
          Expanded(
            child: RefreshIndicator(
              onRefresh: _carregarInventario,
              child: _buildConteudo(),
            ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _exibirMenuAdicionar,
        icon: const Icon(Icons.add),
        label: const Text('Inventário'),
        backgroundColor: const Color(0xFF7C6FE0),
        foregroundColor: Colors.white,
      ),
    );
  }

  Widget _buildInfoResumo(String label, String valor, IconData icone, bool isDark) {
    return Row(
      children: [
        Icon(icone, color: const Color(0xFF7C6FE0), size: 22),
        const SizedBox(width: 8),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: TextStyle(
                fontSize: 11,
                color: isDark ? const Color(0xFF9CA3AF) : const Color(0xFF6F6F85),
              ),
            ),
            Text(
              valor,
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.bold,
                color: isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A),
              ),
            ),
          ],
        ),
      ],
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
                onPressed: _carregarInventario,
                icon: const Icon(Icons.refresh),
                label: const Text('Tentar Novamente'),
              ),
            ],
          ),
        ),
      );
    }

    if (_itensFiltrados.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.inbox_outlined, size: 56, color: Colors.grey.shade400),
              const SizedBox(height: 12),
              Text(
                'Nenhum item encontrado no inventário.',
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
      itemCount: _itensFiltrados.length,
      itemBuilder: (context, index) {
        final item = _itensFiltrados[index];
        final corQtd = _corEstoque(item.quantidadeEmEstoque);

        return Card(
          elevation: 1.5,
          margin: const EdgeInsets.symmetric(vertical: 7),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                // Ícone do Produto
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    color: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.inventory_2, color: Color(0xFF7C6FE0), size: 26),
                ),
                const SizedBox(width: 14),

                // Dados
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item.nomeProduto,
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Theme.of(context).brightness == Brightness.dark
                              ? const Color(0xFFF3F4F6)
                              : const Color(0xFF2E2E3A),
                        ),
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: Theme.of(context).brightness == Brightness.dark
                                  ? const Color(0xFF262A3D)
                                  : Colors.grey.shade100,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Text(
                              item.categoria,
                              style: TextStyle(
                                fontSize: 11,
                                color: Theme.of(context).brightness == Brightness.dark
                                    ? const Color(0xFFE5E7EB)
                                    : Colors.grey.shade800,
                              ),
                            ),
                          ),
                          if (item.cor.isNotEmpty) ...[
                            const SizedBox(width: 6),
                            Text(
                              '• ${item.cor}',
                              style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                            ),
                          ],
                        ],
                      ),
                      if (item.descricao.isNotEmpty) ...[
                        const SizedBox(height: 4),
                        Text(
                          item.descricao,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                        ),
                      ],
                    ],
                  ),
                ),
                const SizedBox(width: 12),

                // Quantidade e Ações
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                      decoration: BoxDecoration(
                        color: corQtd.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: corQtd.withValues(alpha: 0.4)),
                      ),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            '${item.quantidadeEmEstoque}',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: corQtd,
                            ),
                          ),
                          Text(
                            item.quantidadeEmEstoque == 1 ? 'unidade' : 'unidades',
                            style: TextStyle(
                              fontSize: 10,
                              color: corQtd,
                            ),
                          ),
                        ],
                      ),
                    ),
                    PopupMenuButton<String>(
                      icon: const Icon(Icons.more_vert, size: 20, color: Colors.grey),
                      tooltip: 'Opções',
                      onSelected: (val) {
                        if (val == 'movimentar') {
                          _abrirDialogoMovimentacao(produtoPreSelecionado: item);
                        } else if (val == 'excluir') {
                          _confirmarExclusaoProduto(item);
                        }
                      },
                      itemBuilder: (ctx) => [
                        const PopupMenuItem(
                          value: 'movimentar',
                          child: Row(
                            children: [
                              Icon(Icons.swap_horiz, size: 18, color: Color(0xFF7C6FE0)),
                              SizedBox(width: 8),
                              Text('Movimentar Estoque'),
                            ],
                          ),
                        ),
                        const PopupMenuItem(
                          value: 'excluir',
                          child: Row(
                            children: [
                              Icon(Icons.delete_outline, size: 18, color: Color(0xFFE25555)),
                              SizedBox(width: 8),
                              Text('Excluir Produto', style: TextStyle(color: Color(0xFFE25555))),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  void _exibirMenuAdicionar() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 16.0, horizontal: 8.0),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Container(
                  width: 40,
                  height: 4,
                  margin: const EdgeInsets.only(bottom: 16),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
                ListTile(
                  leading: CircleAvatar(
                    backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.15),
                    child: const Icon(Icons.swap_horiz, color: Color(0xFF7C6FE0)),
                  ),
                  title: const Text('Registrar Movimentação (Entrada/Saída)', style: TextStyle(fontWeight: FontWeight.bold)),
                  subtitle: const Text('Entrada de doações ou saída para beneficiários'),
                  onTap: () {
                    Navigator.pop(ctx);
                    _abrirDialogoMovimentacao();
                  },
                ),
                const Divider(),
                ListTile(
                  leading: CircleAvatar(
                    backgroundColor: Colors.green.shade50,
                    child: Icon(Icons.add_box_outlined, color: Colors.green.shade700),
                  ),
                  title: const Text('Cadastrar Novo Produto', style: TextStyle(fontWeight: FontWeight.bold)),
                  subtitle: const Text('Adicionar novo item ao catálogo do estoque'),
                  onTap: () {
                    Navigator.pop(ctx);
                    _abrirDialogoCadastrarProduto();
                  },
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _abrirDialogoMovimentacao({InventarioModel? produtoPreSelecionado}) async {
    if (_itens.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cadastre um produto antes de registrar movimentações.'),
          backgroundColor: Color(0xFFE25555),
        ),
      );
      return;
    }

    final formKey = GlobalKey<FormState>();
    int produtoId = produtoPreSelecionado?.produtoId ?? _itens.first.produtoId;
    String tipo = 'Entrada';
    final qtdCtrl = TextEditingController(text: '1');
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
                  Icon(Icons.swap_horiz, color: Color(0xFF7C6FE0)),
                  SizedBox(width: 8),
                  Text('Movimentar Estoque', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
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
                        // Seleção de Produto
                        DropdownButtonFormField<int>(
                          initialValue: produtoId,
                          isExpanded: true,
                          decoration: const InputDecoration(
                            labelText: 'Produto *',
                            prefixIcon: Icon(Icons.inventory_2_outlined),
                            border: OutlineInputBorder(),
                          ),
                          items: _itens.map((p) {
                            return DropdownMenuItem<int>(
                              value: p.produtoId,
                              child: Text(
                                '${p.nomeProduto} (${p.quantidadeEmEstoque} em estoque)',
                                overflow: TextOverflow.ellipsis,
                              ),
                            );
                          }).toList(),
                          onChanged: (val) {
                            if (val != null) {
                              setDialogState(() => produtoId = val);
                            }
                          },
                        ),
                        const SizedBox(height: 12),

                        // Tipo (Entrada ou Saída)
                        DropdownButtonFormField<String>(
                          initialValue: tipo,
                          isExpanded: true,
                          decoration: const InputDecoration(
                            labelText: 'Tipo de Movimentação *',
                            prefixIcon: Icon(Icons.compare_arrows),
                            border: OutlineInputBorder(),
                          ),
                          items: const [
                            DropdownMenuItem(value: 'Entrada', child: Text('Entrada (Doação Recebida)')),
                            DropdownMenuItem(value: 'Saída', child: Text('Saída (Entrega / Doação Realizada)')),
                          ],
                          onChanged: (val) {
                            if (val != null) {
                              setDialogState(() => tipo = val);
                            }
                          },
                        ),
                        const SizedBox(height: 12),

                        // Quantidade
                        TextFormField(
                          controller: qtdCtrl,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'Quantidade *',
                            prefixIcon: Icon(Icons.format_list_numbered),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) {
                            final n = int.tryParse(v ?? '');
                            if (n == null || n <= 0) {
                              return 'Informe uma quantidade maior que zero';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),

                        // Observação
                        TextFormField(
                          controller: obsCtrl,
                          maxLines: 2,
                          maxLength: 255,
                          decoration: const InputDecoration(
                            labelText: 'Motivo / Observação (Opcional)',
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

                          final dados = {
                            'produtoId': produtoId,
                            'razao': tipo,
                            'quantidade': int.parse(qtdCtrl.text.trim()),
                            'observacao': obsCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            await _apiService.registrarMovimentacaoInventario(token, dados);

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text('Movimentação registrada com sucesso!'),
                                  backgroundColor: Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarInventario();
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

  Future<void> _abrirDialogoCadastrarProduto() async {
    final formKey = GlobalKey<FormState>();
    final nomeCtrl = TextEditingController();
    final corCtrl = TextEditingController();
    final descCtrl = TextEditingController();
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
                  Icon(Icons.add_box_outlined, color: Color(0xFF7C6FE0)),
                  SizedBox(width: 8),
                  Text('Novo Produto', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
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
                          maxLength: 60,
                          decoration: const InputDecoration(
                            labelText: 'Nome do Produto *',
                            hintText: 'Ex: Camiseta Básica Algodão',
                            prefixIcon: Icon(Icons.inventory_2_outlined),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) {
                            if (v == null || v.trim().isEmpty) {
                              return 'Informe o nome do produto';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: corCtrl,
                          maxLength: 15,
                          decoration: const InputDecoration(
                            labelText: 'Cor',
                            hintText: 'Ex: Azul, Preto, Branca',
                            prefixIcon: Icon(Icons.palette_outlined),
                            border: OutlineInputBorder(),
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: descCtrl,
                          maxLength: 255,
                          maxLines: 2,
                          decoration: const InputDecoration(
                            labelText: 'Descrição / Tamanho',
                            hintText: 'Ex: Tamanho M, infantil ou adulto',
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

                          final dados = {
                            'nome': nomeCtrl.text.trim(),
                            'cor': corCtrl.text.trim(),
                            'descricao': descCtrl.text.trim(),
                          };

                          try {
                            final token = await _apiService.getToken();
                            if (token == null) throw Exception('Não autenticado.');

                            await _apiService.criarProduto(token, dados);

                            if (dialogCtx.mounted) {
                              Navigator.pop(dialogCtx);
                            }
                            if (mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text('Produto cadastrado com sucesso!'),
                                  backgroundColor: Color(0xFF2E9E5B),
                                ),
                              );
                              _carregarInventario();
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
                      : const Text('Cadastrar'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  Future<void> _confirmarExclusaoProduto(InventarioModel item) async {
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
        content: Text('Deseja realmente excluir o produto "${item.nomeProduto}"? Itens com histórico de movimentação não poderão ser excluídos.'),
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

      await _apiService.excluirProduto(token, item.produtoId);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Produto excluído com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
          ),
        );
        _carregarInventario();
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

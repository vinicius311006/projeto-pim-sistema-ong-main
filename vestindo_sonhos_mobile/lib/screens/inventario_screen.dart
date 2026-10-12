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
              color: const Color(0xFF7C6FE0).withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: const Color(0xFF7C6FE0).withValues(alpha: 0.25)),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _buildInfoResumo('Produtos Cadastrados', '${_itens.length} itens', Icons.category_outlined),
                Container(height: 30, width: 1, color: Colors.grey.shade300),
                _buildInfoResumo('Total em Estoque', '$_totalEstoqueGeral un', Icons.inventory_2_outlined),
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
                        color: selecionada ? const Color(0xFF7C6FE0) : const Color(0xFF2E2E3A),
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
    );
  }

  Widget _buildInfoResumo(String label, String valor, IconData icone) {
    return Row(
      children: [
        Icon(icone, color: const Color(0xFF7C6FE0), size: 22),
        const SizedBox(width: 8),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(label, style: const TextStyle(fontSize: 11, color: Color(0xFF6F6F85))),
            Text(valor, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Color(0xFF2E2E3A))),
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
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF2E2E3A),
                        ),
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: Colors.grey.shade100,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Text(
                              item.categoria,
                              style: TextStyle(fontSize: 11, color: Colors.grey.shade800),
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

                // Quantidade
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
              ],
            ),
          ),
        );
      },
    );
  }
}

import 'package:flutter/material.dart';
import '../models/dashboard_model.dart';
import '../services/api_service.dart';
import 'beneficiarios_screen.dart';
import 'inventario_screen.dart';
import 'financeiro_screen.dart';
import 'eventos_screen.dart';
import '../widgets/server_config_dialog.dart';

/// Tela Principal (Home) com Dashboard idêntico à Web e Navegação Completa.
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _apiService = ApiService();
  int _indiceAba = 0;
  String _nomeUsuario = 'Voluntário';
  String _cargoUsuario = 'Colaborador';
  String _emailUsuario = '';

  DashboardModel? _dashboard;
  bool _carregandoDashboard = true;
  String? _erroDashboard;

  @override
  void initState() {
    super.initState();
    _carregarPerfil();
  }

  Future<void> _carregarPerfil() async {
    final nome = await _apiService.getUserNome();
    final cargo = await _apiService.getUserCargo();
    final email = await _apiService.getUserEmail();
    if (mounted) {
      setState(() {
        if (nome != null && nome.isNotEmpty) _nomeUsuario = nome;
        if (cargo != null && cargo.isNotEmpty) _cargoUsuario = cargo;
        if (email != null && email.isNotEmpty) _emailUsuario = email;
      });
      if (_cargoUsuario.toLowerCase().contains('admin')) {
        _carregarDashboard();
      }
    }
  }

  Future<void> _carregarDashboard() async {
    setState(() {
      _carregandoDashboard = true;
      _erroDashboard = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null) {
        throw Exception('Sessão expirada. Faça login novamente.');
      }
      final dados = await _apiService.getDashboard(token);
      if (mounted) {
        setState(() {
          _dashboard = dados;
          _carregandoDashboard = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _erroDashboard = e.toString().replaceAll('Exception: ', '');
          _carregandoDashboard = false;
        });
      }
    }
  }

  Future<void> _fazerLogout() async {
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar Saída'),
        content: const Text('Deseja realmente encerrar sua sessão no aplicativo?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFFE25555),
              foregroundColor: Colors.white,
            ),
            child: const Text('Sair'),
          ),
        ],
      ),
    );

    if (confirmar == true) {
      await _apiService.clearAuthData();
      if (!mounted) return;
      Navigator.pushReplacementNamed(context, '/login');
    }
  }

  void _abrirModulo(int index) {
    setState(() {
      _indiceAba = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    final bool isAdmin = _cargoUsuario.toLowerCase().contains('admin');

    // Telas e navegação de acordo com o perfil:
    // Administrador: Dashboard, Inventário, Beneficiários, Financeiro, Eventos
    // Colaborador: Inventário, Beneficiários, Eventos (sem Financeiro e sem Dashboard)
    final telas = isAdmin
        ? [
            _buildDashboardView(),
            const InventarioScreen(),
            const BeneficiariosScreen(),
            const FinanceiroScreen(),
            const EventosScreen(),
          ]
        : [
            const InventarioScreen(),
            const BeneficiariosScreen(),
            const EventosScreen(),
          ];

    final destinos = isAdmin
        ? const [
            NavigationDestination(
              icon: Icon(Icons.home_outlined),
              selectedIcon: Icon(Icons.home),
              label: 'Início',
            ),
            NavigationDestination(
              icon: Icon(Icons.inventory_2_outlined),
              selectedIcon: Icon(Icons.inventory_2),
              label: 'Inventário',
            ),
            NavigationDestination(
              icon: Icon(Icons.diversity_1_outlined),
              selectedIcon: Icon(Icons.diversity_1),
              label: 'Beneficiários',
            ),
            NavigationDestination(
              icon: Icon(Icons.volunteer_activism_outlined),
              selectedIcon: Icon(Icons.volunteer_activism),
              label: 'Financeiro',
            ),
            NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month),
              label: 'Eventos',
            ),
          ]
        : const [
            NavigationDestination(
              icon: Icon(Icons.inventory_2_outlined),
              selectedIcon: Icon(Icons.inventory_2),
              label: 'Inventário',
            ),
            NavigationDestination(
              icon: Icon(Icons.diversity_1_outlined),
              selectedIcon: Icon(Icons.diversity_1),
              label: 'Beneficiários',
            ),
            NavigationDestination(
              icon: Icon(Icons.calendar_month_outlined),
              selectedIcon: Icon(Icons.calendar_month),
              label: 'Eventos',
            ),
          ];

    final int indiceValido = _indiceAba < telas.length ? _indiceAba : 0;

    return Scaffold(
      drawer: _buildDrawerLateral(isAdmin, indiceValido),
      body: telas[indiceValido],
      bottomNavigationBar: NavigationBarTheme(
        data: NavigationBarThemeData(
          labelTextStyle: WidgetStateProperty.resolveWith((states) {
            final isSelected = states.contains(WidgetState.selected);
            return TextStyle(
              fontSize: 11,
              fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
              letterSpacing: -0.2,
            );
          }),
        ),
        child: NavigationBar(
          selectedIndex: indiceValido,
          onDestinationSelected: (index) {
            setState(() {
              _indiceAba = index;
            });
          },
          destinations: destinos,
        ),
      ),
    );
  }

  /// Menu lateral idêntico à Sidebar da Web (_Layout.cshtml)
  Widget _buildDrawerLateral(bool isAdmin, int indiceAtual) {
    return Drawer(
      child: Column(
        children: [
          UserAccountsDrawerHeader(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [Color(0xFF7C6FE0), Color(0xFF9B8DE3)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            currentAccountPicture: CircleAvatar(
              backgroundColor: Colors.white,
              child: Text(
                _nomeUsuario.isNotEmpty ? _nomeUsuario[0].toUpperCase() : 'V',
                style: const TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF7C6FE0),
                ),
              ),
            ),
            accountName: Row(
              children: [
                Text(
                  _nomeUsuario,
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                ),
                const SizedBox(width: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: Colors.white24,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    _cargoUsuario,
                    style: const TextStyle(fontSize: 11, color: Colors.white),
                  ),
                ),
              ],
            ),
            accountEmail: Text(_emailUsuario.isNotEmpty ? _emailUsuario : 'Vestindo Sonhos ONG'),
          ),

          // Itens do menu conforme permissões:
          if (isAdmin) ...[
            _buildItemDrawer(
              icon: Icons.home,
              titulo: 'Home (Dashboard)',
              selecionado: indiceAtual == 0,
              onTap: () {
                Navigator.pop(context);
                _abrirModulo(0);
              },
            ),
            _buildItemDrawer(
              icon: Icons.volunteer_activism,
              titulo: 'Financeiro & Doações',
              selecionado: indiceAtual == 3,
              onTap: () {
                Navigator.pop(context);
                _abrirModulo(3);
              },
            ),
          ],
          _buildItemDrawer(
            icon: Icons.inventory_2,
            titulo: 'Inventário (Estoque)',
            selecionado: isAdmin ? indiceAtual == 1 : indiceAtual == 0,
            onTap: () {
              Navigator.pop(context);
              _abrirModulo(isAdmin ? 1 : 0);
            },
          ),
          _buildItemDrawer(
            icon: Icons.diversity_1,
            titulo: 'Beneficiários',
            selecionado: isAdmin ? indiceAtual == 2 : indiceAtual == 1,
            onTap: () {
              Navigator.pop(context);
              _abrirModulo(isAdmin ? 2 : 1);
            },
          ),
          _buildItemDrawer(
            icon: Icons.calendar_month,
            titulo: 'Eventos & Campanhas',
            selecionado: isAdmin ? indiceAtual == 4 : indiceAtual == 2,
            onTap: () {
              Navigator.pop(context);
              _abrirModulo(isAdmin ? 4 : 2);
            },
          ),

          const Spacer(),
          const Divider(),

          ListTile(
            leading: const Icon(Icons.settings_outlined, color: Color(0xFF6F6F85)),
            title: const Text('Configurar Servidor', style: TextStyle(color: Color(0xFF2C2C35), fontWeight: FontWeight.w600)),
            onTap: () {
              Navigator.pop(context);
              ServerConfigDialog.show(context);
            },
          ),

          ListTile(
            leading: const Icon(Icons.logout, color: Color(0xFFE25555)),
            title: const Text('Sair da Conta', style: TextStyle(color: Color(0xFFE25555), fontWeight: FontWeight.bold)),
            onTap: () {
              Navigator.pop(context);
              _fazerLogout();
            },
          ),
          const SizedBox(height: 8),
        ],
      ),
    );
  }

  Widget _buildItemDrawer({
    required IconData icon,
    required String titulo,
    required bool selecionado,
    required VoidCallback onTap,
  }) {
    return ListTile(
      leading: Icon(icon, color: selecionado ? const Color(0xFF7C6FE0) : const Color(0xFF6F6F85)),
      title: Text(
        titulo,
        style: TextStyle(
          color: selecionado ? const Color(0xFF7C6FE0) : const Color(0xFF2E2E3A),
          fontWeight: selecionado ? FontWeight.bold : FontWeight.normal,
        ),
      ),
      selected: selecionado,
      selectedTileColor: const Color(0xFF7C6FE0).withValues(alpha: 0.08),
      onTap: onTap,
    );
  }

  /// Visão 0: Dashboard Principal
  Widget _buildDashboardView() {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Vestindo Sonhos'),
        actions: [
          IconButton(
            tooltip: 'Atualizar',
            icon: const Icon(Icons.refresh),
            onPressed: _carregarDashboard,
          ),
          IconButton(
            tooltip: 'Sair',
            icon: const Icon(Icons.logout),
            onPressed: _fazerLogout,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _carregarDashboard,
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Banner de Boas-Vindas
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(20.0),
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
                      blurRadius: 8,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Portal Integrado ONG',
                      style: TextStyle(color: Colors.white70, fontSize: 13, letterSpacing: 0.5),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Olá, $_nomeUsuario!',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Acompanhe aqui os indicadores, estoque de doações e eventos em tempo real.',
                      style: TextStyle(color: Colors.white, fontSize: 13),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 22),

              // Métricas Principais (Cards iguais à Home da Web)
              const Text(
                'Visão Geral da ONG',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF2E2E3A)),
              ),
              const SizedBox(height: 12),

              if (_carregandoDashboard)
                const Center(child: Padding(padding: EdgeInsets.all(20), child: CircularProgressIndicator()))
              else if (_erroDashboard != null)
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(color: Colors.red.shade50, borderRadius: BorderRadius.circular(10)),
                  child: Text('Erro ao carregar dados: $_erroDashboard', style: TextStyle(color: Colors.red.shade700)),
                )
              else ...[
                // Grade de métricas
                GridView.count(
                  crossAxisCount: 2,
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisSpacing: 12,
                  mainAxisSpacing: 12,
                  childAspectRatio: 1.5,
                  children: [
                    _buildCardMetrica(
                      titulo: 'Projetos Ativos',
                      valor: '${_dashboard!.projetosAtivos}',
                      icone: Icons.event,
                      cor: const Color(0xFF7C6FE0),
                      onTap: () => _abrirModulo(4),
                    ),
                    _buildCardMetrica(
                      titulo: 'Voluntários',
                      valor: '${_dashboard!.voluntariosCadastrados}',
                      icone: Icons.people,
                      cor: const Color(0xFF2E9E5B),
                      onTap: () => _abrirModulo(4),
                    ),
                    _buildCardMetrica(
                      titulo: 'Itens em Estoque',
                      valor: '${_dashboard!.itensInventarioTotal}',
                      icone: Icons.inventory_2,
                      cor: Colors.orange.shade800,
                      onTap: () => _abrirModulo(1),
                    ),
                    _buildCardMetrica(
                      titulo: 'Beneficiários',
                      valor: '${_dashboard!.beneficiariosCadastrados}',
                      icone: Icons.diversity_1,
                      cor: Colors.pink.shade600,
                      onTap: () => _abrirModulo(2),
                    ),
                  ],
                ),
                const SizedBox(height: 20),

                // Seção de Necessidades da Comunidade (Tags da Web)
                if (_dashboard!.necessidades.isNotEmpty) ...[
                  const Text(
                    'Principais Necessidades da Comunidade',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF2E2E3A)),
                  ),
                  const SizedBox(height: 10),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: _dashboard!.necessidades.map((n) {
                      return Chip(
                        avatar: CircleAvatar(
                          backgroundColor: const Color(0xFF7C6FE0),
                          child: Text('${n.quantidade}', style: const TextStyle(color: Colors.white, fontSize: 11)),
                        ),
                        label: Text('#${n.tag}', style: const TextStyle(fontWeight: FontWeight.bold)),
                        backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
                        side: BorderSide(color: const Color(0xFF7C6FE0).withValues(alpha: 0.3)),
                      );
                    }).toList(),
                  ),
                  const SizedBox(height: 24),
                ],

                // Atalhos Rápidos para os Módulos
                const Text(
                  'Módulos do Sistema',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF2E2E3A)),
                ),
                const SizedBox(height: 12),

                _buildAtalhoLinha(
                  icone: Icons.volunteer_activism,
                  titulo: 'Módulo Financeiro',
                  subtitulo: 'Consulta de saldos de carteiras e doações',
                  onTap: () => _abrirModulo(3),
                ),
                _buildAtalhoLinha(
                  icone: Icons.inventory_2,
                  titulo: 'Módulo de Inventário',
                  subtitulo: 'Controle de roupas, agasalhos e estoque',
                  onTap: () => _abrirModulo(1),
                ),
                _buildAtalhoLinha(
                  icone: Icons.diversity_1,
                  titulo: 'Módulo de Beneficiários',
                  subtitulo: 'Famílias acolhidas e situações',
                  onTap: () => _abrirModulo(2),
                ),
                _buildAtalhoLinha(
                  icone: Icons.calendar_month,
                  titulo: 'Módulo de Eventos',
                  subtitulo: 'Campanhas de arrecadação e voluntariado',
                  onTap: () => _abrirModulo(4),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildCardMetrica({
    required String titulo,
    required String valor,
    required IconData icone,
    required Color cor,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(14),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
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
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                CircleAvatar(
                  radius: 16,
                  backgroundColor: cor.withValues(alpha: 0.12),
                  child: Icon(icone, size: 18, color: cor),
                ),
                Icon(Icons.arrow_forward_ios, size: 12, color: Colors.grey.shade400),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              valor,
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: cor,
              ),
            ),
            Text(
              titulo,
              style: const TextStyle(fontSize: 12, color: Color(0xFF6F6F85)),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAtalhoLinha({
    required IconData icone,
    required String titulo,
    required String subtitulo,
    required VoidCallback onTap,
  }) {
    return Card(
      elevation: 1,
      margin: const EdgeInsets.symmetric(vertical: 6),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        leading: CircleAvatar(
          backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
          child: Icon(icone, color: const Color(0xFF7C6FE0)),
        ),
        title: Text(titulo, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
        subtitle: Text(subtitulo, style: const TextStyle(fontSize: 12)),
        trailing: const Icon(Icons.arrow_forward_ios, size: 14),
        onTap: onTap,
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../main.dart';
import '../models/dashboard_model.dart';
import '../services/api_service.dart';
import '../widgets/server_config_dialog.dart';

/// Dashboard Executivo Principal do Aplicativo Vestindo Sonhos.
/// Sincronizado com o Dashboard Web (KPIs, Gráficos de Estoque/Financeiro,
/// Transações Recentes, Próximos Eventos, Modo Escuro e Configurações SaaS Admin).
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _apiService = ApiService();

  String _nome = 'Voluntário';
  String _email = '';
  String _cargo = 'Voluntário';

  DashboardModel? _dashboard;
  bool _carregando = true;
  String? _erro;

  bool get _isAdmin => _cargo.toLowerCase().contains('admin');

  @override
  void initState() {
    super.initState();
    _inicializar();
  }

  Future<void> _inicializar() async {
    await _carregarDadosUsuario();
    await _carregarDashboard();
  }

  Future<void> _carregarDadosUsuario() async {
    final nome = await _apiService.getUserNome();
    final email = await _apiService.getUserEmail();
    final cargo = await _apiService.getUserCargo();

    if (!mounted) return;
    setState(() {
      if (nome != null && nome.isNotEmpty) _nome = nome;
      if (email != null && email.isNotEmpty) _email = email;
      if (cargo != null && cargo.isNotEmpty) _cargo = cargo;
    });
  }

  Future<void> _carregarDashboard() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final token = await _apiService.getToken();
      if (token == null || token.isEmpty) {
        if (!mounted) return;
        Navigator.pushReplacementNamed(context, '/login');
        return;
      }

      final dados = await _apiService.getDashboard(token);
      if (mounted) {
        setState(() {
          _dashboard = dados;
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

  Future<void> _fazerLogout() async {
    await _apiService.clearAuthData();
    if (!mounted) return;
    Navigator.pushReplacementNamed(context, '/login');
  }

  String _formatarMoeda(double valor) {
    return 'R\$ ${valor.toStringAsFixed(2).replaceAll('.', ',')}';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'Dashboard Executivo',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
        ),
        actions: [
          IconButton(
            icon: Icon(isDark ? Icons.light_mode_rounded : Icons.dark_mode_outlined),
            tooltip: isDark ? 'Modo Claro' : 'Modo Escuro',
            onPressed: toggleAppTheme,
          ),
          if (_isAdmin)
            IconButton(
              icon: const Icon(Icons.tune_rounded),
              tooltip: 'Configurações SaaS (Administrador)',
              onPressed: () async {
                await Navigator.pushNamed(context, '/configuracoes');
                _carregarDashboard();
              },
            ),
          IconButton(
            icon: const Icon(Icons.refresh_rounded),
            tooltip: 'Atualizar Indicadores',
            onPressed: _carregarDashboard,
          ),
        ],
      ),
      drawer: _buildDrawer(context, isDark),
      body: RefreshIndicator(
        onRefresh: _carregarDashboard,
        child: _buildBody(theme, isDark),
      ),
    );
  }

  Widget _buildDrawer(BuildContext context, bool isDark) {
    final textColor = isDark ? const Color(0xFFE5E7EB) : const Color(0xFF2E2E3A);

    return Drawer(
      child: Column(
        children: [
          UserAccountsDrawerHeader(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [Color(0xFF7C6FE0), Color(0xFF5A4FCF)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            currentAccountPicture: CircleAvatar(
              backgroundColor: Colors.white,
              child: Text(
                _nome.isNotEmpty ? _nome[0].toUpperCase() : 'V',
                style: const TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF7C6FE0),
                ),
              ),
            ),
            accountName: Row(
              children: [
                Flexible(
                  child: Text(
                    _nome,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                ),
                const SizedBox(width: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.22),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Text(
                    _isAdmin ? 'Admin' : _cargo,
                    style: const TextStyle(fontSize: 10, color: Colors.white, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            accountEmail: Text(
              _email.isNotEmpty ? _email : _cargo,
              style: const TextStyle(color: Colors.white70, fontSize: 13),
            ),
          ),
          Expanded(
            child: ListView(
              padding: EdgeInsets.zero,
              children: [
                _buildDrawerItem(
                  icon: Icons.dashboard_rounded,
                  label: 'Dashboard Principal',
                  selected: true,
                  textColor: textColor,
                  onTap: () => Navigator.pop(context),
                ),
                _buildDrawerItem(
                  icon: Icons.people_alt_rounded,
                  label: 'Beneficiários',
                  textColor: textColor,
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.pushNamed(context, '/beneficiarios').then((_) => _carregarDashboard());
                  },
                ),
                _buildDrawerItem(
                  icon: Icons.inventory_2_rounded,
                  label: 'Inventário & Estoque',
                  textColor: textColor,
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.pushNamed(context, '/inventario').then((_) => _carregarDashboard());
                  },
                ),
                _buildDrawerItem(
                  icon: Icons.account_balance_wallet_rounded,
                  label: 'Financeiro & Doações',
                  textColor: textColor,
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.pushNamed(context, '/financeiro').then((_) => _carregarDashboard());
                  },
                ),
                _buildDrawerItem(
                  icon: Icons.event_available_rounded,
                  label: 'Eventos & Ações',
                  textColor: textColor,
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.pushNamed(context, '/eventos').then((_) => _carregarDashboard());
                  },
                ),
                const Divider(),
                SwitchListTile(
                  secondary: Icon(
                    isDark ? Icons.dark_mode_rounded : Icons.light_mode_rounded,
                    color: const Color(0xFF7C6FE0),
                  ),
                  title: Text(
                    'Modo Escuro',
                    style: TextStyle(fontWeight: FontWeight.w600, color: textColor),
                  ),
                  value: isDark,
                  activeThumbColor: const Color(0xFF7C6FE0),
                  onChanged: (_) => toggleAppTheme(),
                ),
                if (_isAdmin)
                  _buildDrawerItem(
                    icon: Icons.tune_rounded,
                    label: 'Configurações do Sistema (SaaS)',
                    textColor: textColor,
                    onTap: () {
                      Navigator.pop(context);
                      Navigator.pushNamed(context, '/configuracoes').then((_) => _carregarDashboard());
                    },
                  ),
                _buildDrawerItem(
                  icon: Icons.dns_rounded,
                  label: 'Configurar Servidor / IP',
                  textColor: textColor,
                  onTap: () async {
                    Navigator.pop(context);
                    final alterou = await ServerConfigDialog.show(context);
                    if (alterou == true) {
                      _carregarDashboard();
                    }
                  },
                ),
                const Divider(),
                ListTile(
                  leading: const Icon(Icons.logout_rounded, color: Color(0xFFE25555)),
                  title: const Text(
                    'Sair da Conta',
                    style: TextStyle(color: Color(0xFFE25555), fontWeight: FontWeight.bold),
                  ),
                  onTap: _fazerLogout,
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: Text(
              'Vestindo Sonhos • Gestão ONG v2.0',
              style: TextStyle(fontSize: 12, color: Colors.grey.shade500),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDrawerItem({
    required IconData icon,
    required String label,
    required VoidCallback onTap,
    required Color textColor,
    bool selected = false,
  }) {
    return ListTile(
      leading: Icon(
        icon,
        color: selected ? const Color(0xFF7C6FE0) : textColor.withValues(alpha: 0.75),
      ),
      title: Text(
        label,
        style: TextStyle(
          fontWeight: selected ? FontWeight.bold : FontWeight.normal,
          color: selected ? const Color(0xFF7C6FE0) : textColor,
        ),
      ),
      selected: selected,
      onTap: onTap,
    );
  }

  Widget _buildBody(ThemeData theme, bool isDark) {
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
              const Icon(Icons.cloud_off_rounded, size: 56, color: Color(0xFFE25555)),
              const SizedBox(height: 12),
              Text(
                'Não foi possível sincronizar o Dashboard',
                style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 6),
              Text(_erro!, textAlign: TextAlign.center),
              const SizedBox(height: 16),
              Wrap(
                spacing: 10,
                children: [
                  ElevatedButton.icon(
                    onPressed: _carregarDashboard,
                    icon: const Icon(Icons.refresh),
                    label: const Text('Tentar Novamente'),
                  ),
                  OutlinedButton.icon(
                    onPressed: () => ServerConfigDialog.show(context),
                    icon: const Icon(Icons.settings_ethernet),
                    label: const Text('Configurar IP'),
                  ),
                ],
              ),
            ],
          ),
        ),
      );
    }

    final d = _dashboard!;
    final textPrimary = isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A);
    final textSecondary = isDark ? const Color(0xFF9CA3AF) : const Color(0xFF6F6F85);

    return SingleChildScrollView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 1. Header Executivo
          _buildHeaderCard(d),
          const SizedBox(height: 20),

          // 2. KPIs Principais (4 Indicadores do Topo)
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Indicadores Chave (KPIs)',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: textPrimary),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: const Color(0xFF2E9E5B).withValues(alpha: 0.14),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: const Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.sync_rounded, size: 14, color: Color(0xFF2E9E5B)),
                    SizedBox(width: 4),
                    Text(
                      'Tempo Real',
                      style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Color(0xFF2E9E5B)),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),

          GridView.count(
            crossAxisCount: 2,
            crossAxisSpacing: 12,
            mainAxisSpacing: 12,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            childAspectRatio: 1.32,
            children: [
              _buildKpiCard(
                titulo: 'Beneficiários',
                valor: '${d.beneficiariosCadastrados}',
                subtitulo: '+${d.totalDependentes} dependentes',
                icone: Icons.people_alt_rounded,
                cor: const Color(0xFF7C6FE0),
                isDark: isDark,
                onTap: () => Navigator.pushNamed(context, '/beneficiarios').then((_) => _carregarDashboard()),
              ),
              _buildKpiCard(
                titulo: 'Voluntários Ativos',
                valor: '${d.voluntariosCadastrados}',
                subtitulo: '${d.projetosAtivos} eventos ativos',
                icone: Icons.volunteer_activism_rounded,
                cor: const Color(0xFF00B4D8),
                isDark: isDark,
                onTap: () => Navigator.pushNamed(context, '/eventos').then((_) => _carregarDashboard()),
              ),
              _buildKpiCard(
                titulo: 'Saldo em Caixa',
                valor: _formatarMoeda(d.saldoTotal),
                subtitulo: 'Doações: ${_formatarMoeda(d.totalDoacoes)}',
                icone: Icons.account_balance_wallet_rounded,
                cor: const Color(0xFF2E9E5B),
                isDark: isDark,
                onTap: () => Navigator.pushNamed(context, '/financeiro').then((_) => _carregarDashboard()),
              ),
              _buildKpiCard(
                titulo: 'Inventário',
                valor: '${d.itensInventarioTotal} itens',
                subtitulo: '${d.totalUnidadesEstoque} un. em estoque',
                icone: Icons.inventory_2_rounded,
                cor: const Color(0xFFF59E0B),
                isDark: isDark,
                onTap: () => Navigator.pushNamed(context, '/inventario').then((_) => _carregarDashboard()),
              ),
            ],
          ),
          const SizedBox(height: 22),

          // 3. Gráfico / Resumo de Evolução Financeira & Meta Mensal
          _buildGraficoFinanceiroCard(d, isDark, textPrimary, textSecondary),
          const SizedBox(height: 18),

          // 4. Gráfico de Distribuição do Inventário por Categoria
          _buildGraficoInventarioCard(d, isDark, textPrimary, textSecondary),
          const SizedBox(height: 18),

          // 5. Demandas Prioritárias da Comunidade (Somente dados reais do banco)
          _buildDemandasComunidadeCard(d, isDark, textPrimary, textSecondary),
          const SizedBox(height: 22),

          // 6. Tabela Rápida: Últimas Transações Financeiras
          _buildUltimasTransacoesSection(d, isDark, textPrimary, textSecondary),
          const SizedBox(height: 22),

          // 7. Tabela Rápida: Próximos Eventos Agendados
          _buildProximosEventosSection(d, isDark, textPrimary, textSecondary),
          const SizedBox(height: 22),

          // 8. Acesso Rápido aos Módulos
          Text(
            'Acesso Rápido aos Módulos',
            style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: textPrimary),
          ),
          const SizedBox(height: 10),
          _buildModuloTile(
            titulo: 'Beneficiários Atendidos',
            subtitulo: 'Cadastro social, dependentes e acompanhamento de famílias',
            icone: Icons.family_restroom_rounded,
            cor: const Color(0xFF7C6FE0),
            rota: '/beneficiarios',
            isDark: isDark,
          ),
          _buildModuloTile(
            titulo: 'Inventário & Estoque',
            subtitulo: 'Controle de doações físicas, entradas e saídas de roupas',
            icone: Icons.checkroom_rounded,
            cor: const Color(0xFF00B4D8),
            rota: '/inventario',
            isDark: isDark,
          ),
          _buildModuloTile(
            titulo: 'Gestão Financeira & Doações',
            subtitulo: 'Carteiras, receitas, despesas e balanço transparente',
            icone: Icons.monetization_on_rounded,
            cor: const Color(0xFF2E9E5B),
            rota: '/financeiro',
            isDark: isDark,
          ),
          _buildModuloTile(
            titulo: 'Eventos & Campanhas',
            subtitulo: 'Cronograma de ações sociais e escala de voluntários',
            icone: Icons.event_available_rounded,
            cor: Colors.orange.shade700,
            rota: '/eventos',
            isDark: isDark,
          ),
          if (_isAdmin)
            _buildModuloTile(
              titulo: 'Configurações do Sistema (SaaS)',
              subtitulo: 'Exclusivo Administrador: metas, segurança e parâmetros da ONG',
              icone: Icons.tune_rounded,
              cor: const Color(0xFFE25555),
              rota: '/configuracoes',
              isDark: isDark,
            ),
          const SizedBox(height: 16),
        ],
      ),
    );
  }

  Widget _buildHeaderCard(DashboardModel d) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF7C6FE0), Color(0xFF5B4FCF)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF7C6FE0).withValues(alpha: 0.28),
            blurRadius: 12,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 22,
                backgroundColor: Colors.white.withValues(alpha: 0.2),
                child: const Icon(Icons.volunteer_activism_rounded, color: Colors.white, size: 24),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      d.nomeOrganizacao,
                      style: const TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.w600),
                    ),
                    Text(
                      'Olá, $_nome!',
                      style: const TextStyle(color: Colors.white, fontSize: 19, fontWeight: FontWeight.bold),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.18),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  _cargo,
                  style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          const Divider(color: Colors.white24, height: 1),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text('Saldo Consolidado', style: TextStyle(color: Colors.white70, fontSize: 11)),
                  Text(
                    _formatarMoeda(d.saldoTotal),
                    style: const TextStyle(color: Colors.white, fontSize: 17, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  const Text('Peças Doadas (Saídas)', style: TextStyle(color: Colors.white70, fontSize: 11)),
                  Text(
                    '${d.totalItensDoadosSaida} unidades',
                    style: const TextStyle(color: Colors.white, fontSize: 15, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildKpiCard({
    required String titulo,
    required String valor,
    required String subtitulo,
    required IconData icone,
    required Color cor,
    required bool isDark,
    required VoidCallback onTap,
  }) {
    final cardBg = isDark ? const Color(0xFF1F2232) : Colors.white;
    final borderColor = isDark ? const Color(0xFF2D324A) : Colors.grey.shade200;
    final titleColor = isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A);
    final subColor = isDark ? const Color(0xFF9CA3AF) : const Color(0xFF6F6F85);

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: cardBg,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: borderColor),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: cor.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Icon(icone, color: cor, size: 20),
                ),
                Icon(Icons.arrow_forward_ios_rounded, size: 13, color: subColor),
              ],
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  valor,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: titleColor),
                ),
                Text(
                  titulo,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: subColor),
                ),
                const SizedBox(height: 2),
                Text(
                  subtitulo,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 10, color: cor, fontWeight: FontWeight.w600),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildGraficoFinanceiroCard(
    DashboardModel d,
    bool isDark,
    Color textPrimary,
    Color textSecondary,
  ) {
    final totalMov = (d.entradasMesAtual + d.saidasMesAtual);
    final pctEntradas = totalMov > 0 ? (d.entradasMesAtual / totalMov).clamp(0.0, 1.0) : 0.5;
    final pctMeta = d.metaArrecadacaoMensal > 0
        ? (d.totalDoacoes / d.metaArrecadacaoMensal).clamp(0.0, 1.0)
        : 0.0;

    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.bar_chart_rounded, color: Color(0xFF2E9E5B), size: 22),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Fluxo Financeiro (Entradas vs Saídas)',
                    style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: textPrimary),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Row(
              children: [
                Expanded(
                  child: Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF2E9E5B).withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Total Entradas',
                          style: TextStyle(fontSize: 11, color: Color(0xFF2E9E5B), fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          _formatarMoeda(d.entradasMesAtual),
                          style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: textPrimary),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFFE25555).withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Total Saídas',
                          style: TextStyle(fontSize: 11, color: Color(0xFFE25555), fontWeight: FontWeight.w600),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          _formatarMoeda(d.saidasMesAtual),
                          style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: textPrimary),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: SizedBox(
                height: 10,
                child: Row(
                  children: [
                    Expanded(
                      flex: (pctEntradas * 100).round().clamp(1, 99),
                      child: Container(color: const Color(0xFF2E9E5B)),
                    ),
                    Expanded(
                      flex: ((1 - pctEntradas) * 100).round().clamp(1, 99),
                      child: Container(color: const Color(0xFFE25555)),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 14),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Meta Mensal de Doações (${_formatarMoeda(d.metaArrecadacaoMensal)})',
                  style: TextStyle(fontSize: 12, color: textSecondary),
                ),
                Text(
                  '${(pctMeta * 100).toStringAsFixed(0)}%',
                  style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Color(0xFF7C6FE0)),
                ),
              ],
            ),
            const SizedBox(height: 6),
            ClipRRect(
              borderRadius: BorderRadius.circular(6),
              child: LinearProgressIndicator(
                value: pctMeta,
                minHeight: 7,
                backgroundColor: isDark ? const Color(0xFF2D324A) : Colors.grey.shade200,
                valueColor: const AlwaysStoppedAnimation<Color>(Color(0xFF7C6FE0)),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildGraficoInventarioCard(
    DashboardModel d,
    bool isDark,
    Color textPrimary,
    Color textSecondary,
  ) {
    final categorias = d.inventarioPorCategoria;
    final totalUn = categorias.fold<int>(0, (s, item) => s + item.quantidade);
    final cores = [
      const Color(0xFF7C6FE0),
      const Color(0xFF00B4D8),
      const Color(0xFF2E9E5B),
      const Color(0xFFF59E0B),
      const Color(0xFFE25555),
    ];

    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.pie_chart_outline_rounded, color: Color(0xFF00B4D8), size: 22),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Distribuição de Estoque por Categoria',
                    style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: textPrimary),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if (categorias.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 12.0),
                child: Text(
                  'Nenhum item em estoque no momento.',
                  style: TextStyle(fontSize: 13, color: textSecondary),
                ),
              )
            else
              ...categorias.asMap().entries.map((entry) {
                final idx = entry.key;
                final item = entry.value;
                final cor = cores[idx % cores.length];
                final pct = totalUn > 0 ? (item.quantidade / totalUn).clamp(0.0, 1.0) : 0.0;

                return Padding(
                  padding: const EdgeInsets.only(bottom: 10.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            item.categoria,
                            style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: textPrimary),
                          ),
                          Text(
                            '${item.quantidade} un. (${(pct * 100).toStringAsFixed(0)}%)',
                            style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: cor),
                          ),
                        ],
                      ),
                      const SizedBox(height: 5),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(6),
                        child: LinearProgressIndicator(
                          value: pct,
                          minHeight: 7,
                          backgroundColor: isDark ? const Color(0xFF2D324A) : Colors.grey.shade200,
                          valueColor: AlwaysStoppedAnimation<Color>(cor),
                        ),
                      ),
                    ],
                  ),
                );
              }),
          ],
        ),
      ),
    );
  }

  Widget _buildDemandasComunidadeCard(
    DashboardModel d,
    bool isDark,
    Color textPrimary,
    Color textSecondary,
  ) {
    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.favorite_border_rounded, color: Color(0xFF7C6FE0), size: 22),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Demandas Prioritárias da Comunidade',
                    style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: textPrimary),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if (d.necessidades.isEmpty)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: isDark ? const Color(0xFF262A3D) : const Color(0xFFF8F9FC),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  children: [
                    Icon(Icons.inbox_outlined, size: 32, color: textSecondary),
                    const SizedBox(height: 6),
                    Text(
                      'Nenhuma demanda registrada no banco de dados.',
                      style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: textPrimary),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Cadastre beneficiários com observações (ex: #Agasalho, #Calçados) para visualizar os pedidos reais.',
                      style: TextStyle(fontSize: 11, color: textSecondary),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              )
            else
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: d.necessidades.map((nec) {
                  return Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
                    decoration: BoxDecoration(
                      color: const Color(0xFF7C6FE0).withValues(alpha: 0.14),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: const Color(0xFF7C6FE0).withValues(alpha: 0.3)),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          '#${nec.tag}',
                          style: const TextStyle(
                            color: Color(0xFF7C6FE0),
                            fontWeight: FontWeight.bold,
                            fontSize: 13,
                          ),
                        ),
                        const SizedBox(width: 6),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                          decoration: BoxDecoration(
                            color: const Color(0xFF7C6FE0),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Text(
                            '${nec.quantidade}',
                            style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                  );
                }).toList(),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildUltimasTransacoesSection(
    DashboardModel d,
    bool isDark,
    Color textPrimary,
    Color textSecondary,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Últimas Transações Financeiras',
              style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: textPrimary),
            ),
            TextButton(
              onPressed: () => Navigator.pushNamed(context, '/financeiro').then((_) => _carregarDashboard()),
              child: const Text('Ver Tudo'),
            ),
          ],
        ),
        if (d.ultimasTransacoes.isEmpty)
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Center(
                child: Text(
                  'Nenhuma transação financeira registrada.',
                  style: TextStyle(color: textSecondary),
                ),
              ),
            ),
          )
        else
          ...d.ultimasTransacoes.map((t) {
            final isEntrada = t.razao.toLowerCase().contains('entrada');
            final cor = isEntrada ? const Color(0xFF2E9E5B) : const Color(0xFFE25555);

            return Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: ListTile(
                dense: true,
                leading: CircleAvatar(
                  radius: 18,
                  backgroundColor: cor.withValues(alpha: 0.14),
                  child: Icon(
                    isEntrada ? Icons.south_west_rounded : Icons.north_east_rounded,
                    color: cor,
                    size: 18,
                  ),
                ),
                title: Text(
                  t.categoria,
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: textPrimary),
                ),
                subtitle: Text(
                  '${t.carteira} • ${t.observacao?.isNotEmpty == true ? t.observacao : t.razao}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 12, color: textSecondary),
                ),
                trailing: Text(
                  '${isEntrada ? '+' : '-'} ${_formatarMoeda(t.valor)}',
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: cor),
                ),
              ),
            );
          }),
      ],
    );
  }

  Widget _buildProximosEventosSection(
    DashboardModel d,
    bool isDark,
    Color textPrimary,
    Color textSecondary,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Próximos Eventos Agendados',
              style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: textPrimary),
            ),
            TextButton(
              onPressed: () => Navigator.pushNamed(context, '/eventos').then((_) => _carregarDashboard()),
              child: const Text('Ver Todos'),
            ),
          ],
        ),
        if (d.proximosEventos.isEmpty)
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Center(
                child: Text(
                  'Nenhum evento agendado no momento.',
                  style: TextStyle(color: textSecondary),
                ),
              ),
            ),
          )
        else
          ...d.proximosEventos.map((ev) {
            return Card(
              margin: const EdgeInsets.only(bottom: 10),
              child: ListTile(
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                leading: CircleAvatar(
                  backgroundColor: const Color(0xFF7C6FE0).withValues(alpha: 0.15),
                  child: const Icon(Icons.event, color: Color(0xFF7C6FE0)),
                ),
                title: Text(
                  ev.nome,
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: textPrimary),
                ),
                subtitle: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const SizedBox(height: 4),
                    Text('📅 ${ev.dataInicioFormatada}', style: TextStyle(fontSize: 12, color: textSecondary)),
                    if (ev.local != null && ev.local!.isNotEmpty)
                      Text('📍 ${ev.local}', style: TextStyle(fontSize: 12, color: textSecondary)),
                  ],
                ),
                trailing: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: const Color(0xFF00B4D8).withValues(alpha: 0.14),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    '${ev.totalVoluntarios} vol.',
                    style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Color(0xFF00B4D8)),
                  ),
                ),
                onTap: () => Navigator.pushNamed(context, '/eventos').then((_) => _carregarDashboard()),
              ),
            );
          }),
      ],
    );
  }

  Widget _buildModuloTile({
    required String titulo,
    required String subtitulo,
    required IconData icone,
    required Color cor,
    required String rota,
    required bool isDark,
  }) {
    final textPrimary = isDark ? const Color(0xFFF3F4F6) : const Color(0xFF2E2E3A);
    final textSecondary = isDark ? const Color(0xFF9CA3AF) : const Color(0xFF6F6F85);

    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        leading: Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: cor.withValues(alpha: 0.14),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Icon(icone, color: cor, size: 24),
        ),
        title: Text(
          titulo,
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: textPrimary),
        ),
        subtitle: Text(
          subtitulo,
          style: TextStyle(fontSize: 12, color: textSecondary),
        ),
        trailing: Icon(Icons.chevron_right_rounded, color: textSecondary),
        onTap: () => Navigator.pushNamed(context, rota).then((_) => _carregarDashboard()),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../main.dart';
import '../services/api_service.dart';
import '../widgets/server_config_dialog.dart';

/// Tela de Configurações do Sistema (Padrão SaaS) exclusiva para Administradores.
class ConfiguracoesSaasScreen extends StatefulWidget {
  const ConfiguracoesSaasScreen({super.key});

  @override
  State<ConfiguracoesSaasScreen> createState() => _ConfiguracoesSaasScreenState();
}

class _ConfiguracoesSaasScreenState extends State<ConfiguracoesSaasScreen> {
  final _apiService = ApiService();
  final _formKey = GlobalKey<FormState>();

  final _nomeOrgCtrl = TextEditingController(text: 'ONG Vestindo Sonhos');
  final _cnpjCtrl = TextEditingController(text: '12.345.678/0001-90');
  final _emailCtrl = TextEditingController(text: 'contato@vestindosonhos.org.br');
  final _telefoneCtrl = TextEditingController(text: '(11) 98765-4321');
  final _enderecoCtrl = TextEditingController(text: 'Av. Paulista, 1500 - São Paulo/SP');
  final _metaCtrl = TextEditingController(text: '15000');
  final _estoqueMinCtrl = TextEditingController(text: '10');
  final _sessaoCtrl = TextEditingController(text: '30');

  bool _exigirTrocaSenha = true;
  bool _notificarDoacoes = true;
  bool _habilitarApiMobile = true;

  bool _carregando = true;
  bool _salvando = false;
  bool _isAdmin = false;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregarDados();
  }

  @override
  void dispose() {
    _nomeOrgCtrl.dispose();
    _cnpjCtrl.dispose();
    _emailCtrl.dispose();
    _telefoneCtrl.dispose();
    _enderecoCtrl.dispose();
    _metaCtrl.dispose();
    _estoqueMinCtrl.dispose();
    _sessaoCtrl.dispose();
    super.dispose();
  }

  Future<void> _carregarDados() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });

    try {
      final cargo = await _apiService.getUserCargo() ?? '';
      final admin = cargo.toLowerCase().contains('admin');
      if (!admin) {
        if (mounted) {
          setState(() {
            _isAdmin = false;
            _carregando = false;
          });
        }
        return;
      }

      _isAdmin = true;
      final token = await _apiService.getToken();
      if (token == null) throw Exception('Não autenticado.');

      final cfg = await _apiService.getConfiguracoesSaas(token);
      if (mounted) {
        setState(() {
          _nomeOrgCtrl.text = (cfg['nomeOrganizacao'] ?? cfg['NomeOrganizacao'] ?? 'ONG Vestindo Sonhos').toString();
          _cnpjCtrl.text = (cfg['cnpj'] ?? cfg['Cnpj'] ?? '12.345.678/0001-90').toString();
          _emailCtrl.text = (cfg['emailInstitucional'] ?? cfg['EmailInstitucional'] ?? '').toString();
          _telefoneCtrl.text = (cfg['telefoneSuporte'] ?? cfg['TelefoneSuporte'] ?? '').toString();
          _enderecoCtrl.text = (cfg['enderecoSede'] ?? cfg['EnderecoSede'] ?? '').toString();
          _metaCtrl.text = (cfg['metaArrecadacaoMensal'] ?? cfg['MetaArrecadacaoMensal'] ?? 15000).toString();
          _estoqueMinCtrl.text = (cfg['alertaEstoqueMinimo'] ?? cfg['AlertaEstoqueMinimo'] ?? 10).toString();
          _sessaoCtrl.text = (cfg['tempoSessaoMinutos'] ?? cfg['TempoSessaoMinutos'] ?? 30).toString();
          _exigirTrocaSenha = (cfg['exigirTrocaSenhaNovosUsuarios'] ?? cfg['ExigirTrocaSenhaNovosUsuarios'] ?? true) == true;
          _notificarDoacoes = (cfg['notificarNovasDoacoes'] ?? cfg['NotificarNovasDoacoes'] ?? true) == true;
          _habilitarApiMobile = (cfg['habilitarApiMobile'] ?? cfg['HabilitarApiMobile'] ?? true) == true;
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

  Future<void> _salvarConfiguracoes() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _salvando = true);

    try {
      final token = await _apiService.getToken();
      if (token == null) throw Exception('Sessão expirada.');

      final dados = {
        'nomeOrganizacao': _nomeOrgCtrl.text.trim(),
        'cnpj': _cnpjCtrl.text.trim(),
        'emailInstitucional': _emailCtrl.text.trim(),
        'telefoneSuporte': _telefoneCtrl.text.trim(),
        'enderecoSede': _enderecoCtrl.text.trim(),
        'metaArrecadacaoMensal': double.tryParse(_metaCtrl.text.replaceAll(',', '.')) ?? 15000.0,
        'alertaEstoqueMinimo': int.tryParse(_estoqueMinCtrl.text) ?? 10,
        'tempoSessaoMinutos': int.tryParse(_sessaoCtrl.text) ?? 30,
        'exigirTrocaSenhaNovosUsuarios': _exigirTrocaSenha,
        'notificarNovasDoacoes': _notificarDoacoes,
        'habilitarApiMobile': _habilitarApiMobile,
      };

      await _apiService.salvarConfiguracoesSaas(token, dados);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Configurações SaaS salvas com sucesso!'),
            backgroundColor: Color(0xFF2E9E5B),
            behavior: SnackBarBehavior.floating,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceAll('Exception: ', '')),
            backgroundColor: const Color(0xFFE25555),
            behavior: SnackBarBehavior.floating,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _salvando = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(
        centerTitle: false,
        title: const Text('Configurações SaaS', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        actions: [
          IconButton(
            icon: Icon(isDark ? Icons.light_mode_rounded : Icons.dark_mode_outlined),
            tooltip: isDark ? 'Modo Claro' : 'Modo Escuro',
            onPressed: toggleAppTheme,
          ),
        ],
      ),
      body: SafeArea(
        child: _carregando
          ? const Center(child: CircularProgressIndicator())
          : !_isAdmin
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.lock_person_rounded, size: 64, color: Color(0xFFE25555)),
                        const SizedBox(height: 16),
                        Text(
                          'Acesso Restrito ao Administrador',
                          style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Por regra de negócio da ONG, as Configurações Globais do Sistema (SaaS) são exclusivas para usuários com perfil de Administrador.',
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ),
                  ),
                )
              : SingleChildScrollView(
                  padding: const EdgeInsets.all(16.0),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        if (_erro != null)
                          Container(
                            margin: const EdgeInsets.only(bottom: 16),
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: const Color(0xFFE25555).withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Text(_erro!, style: const TextStyle(color: Color(0xFFE25555))),
                          ),

                        // Card 1: Aparência e Tema
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Icon(Icons.palette_outlined, color: theme.colorScheme.primary),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        'Aparência & Tema do Sistema',
                                        style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                                      ),
                                    ),
                                  ],
                                ),
                                const Divider(height: 20),
                                SwitchListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: const Text('Modo Escuro (Dark Mode)', style: TextStyle(fontWeight: FontWeight.w600)),
                                  subtitle: const Text('Ativa a paleta escura confortável em todas as telas do aplicativo'),
                                  value: isDark,
                                  activeThumbColor: const Color(0xFF7C6FE0),
                                  onChanged: (_) => toggleAppTheme(),
                                ),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        // Card 2: Organização e Metas SaaS
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Icon(Icons.business_rounded, color: theme.colorScheme.primary),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        'Dados da Organização & Metas',
                                        style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                                      ),
                                    ),
                                  ],
                                ),
                                const Divider(height: 20),
                                TextFormField(
                                  controller: _nomeOrgCtrl,
                                  decoration: const InputDecoration(
                                    labelText: 'Nome da Organização *',
                                    prefixIcon: Icon(Icons.volunteer_activism_outlined),
                                    border: OutlineInputBorder(),
                                  ),
                                  validator: (v) => (v == null || v.trim().isEmpty) ? 'Obrigatório' : null,
                                ),
                                const SizedBox(height: 12),
                                TextFormField(
                                  controller: _cnpjCtrl,
                                  decoration: const InputDecoration(
                                    labelText: 'CNPJ Institucional',
                                    prefixIcon: Icon(Icons.badge_outlined),
                                    border: OutlineInputBorder(),
                                  ),
                                ),
                                const SizedBox(height: 12),
                                TextFormField(
                                  controller: _emailCtrl,
                                  decoration: const InputDecoration(
                                    labelText: 'E-mail Institucional',
                                    prefixIcon: Icon(Icons.email_outlined),
                                    border: OutlineInputBorder(),
                                  ),
                                ),
                                const SizedBox(height: 12),
                                Row(
                                  children: [
                                    Expanded(
                                      child: TextFormField(
                                        controller: _metaCtrl,
                                        keyboardType: TextInputType.number,
                                        decoration: const InputDecoration(
                                          labelText: 'Meta Mensal (R\$)',
                                          prefixIcon: Icon(Icons.savings_outlined),
                                          border: OutlineInputBorder(),
                                        ),
                                      ),
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: TextFormField(
                                        controller: _estoqueMinCtrl,
                                        keyboardType: TextInputType.number,
                                        decoration: const InputDecoration(
                                          labelText: 'Estoque Mín. Alerta',
                                          prefixIcon: Icon(Icons.inventory_2_outlined),
                                          border: OutlineInputBorder(),
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        // Card 3: Segurança e Governança
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Icon(Icons.shield_outlined, color: theme.colorScheme.primary),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        'Segurança, Governança & API',
                                        style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                                      ),
                                    ),
                                  ],
                                ),
                                const Divider(height: 20),
                                SwitchListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: const Text('Exigir troca de senha no 1º acesso'),
                                  subtitle: const Text('Obriga novos voluntários a definirem senha própria'),
                                  value: _exigirTrocaSenha,
                                  activeThumbColor: const Color(0xFF7C6FE0),
                                  onChanged: (v) => setState(() => _exigirTrocaSenha = v),
                                ),
                                SwitchListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: const Text('Notificar novas doações'),
                                  subtitle: const Text('Destaca alertas de entradas financeiras no painel'),
                                  value: _notificarDoacoes,
                                  activeThumbColor: const Color(0xFF7C6FE0),
                                  onChanged: (v) => setState(() => _notificarDoacoes = v),
                                ),
                                SwitchListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: const Text('Integração Mobile REST API Ativa'),
                                  subtitle: const Text('Habilita sincronização em tempo real com dispositivos'),
                                  value: _habilitarApiMobile,
                                  activeThumbColor: const Color(0xFF7C6FE0),
                                  onChanged: (v) => setState(() => _habilitarApiMobile = v),
                                ),
                                const SizedBox(height: 8),
                                OutlinedButton.icon(
                                  onPressed: () => ServerConfigDialog.show(context),
                                  icon: const Icon(Icons.dns_outlined),
                                  label: const Text('Configurar Endereço IP / Servidor da API'),
                                ),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 24),

                        ElevatedButton.icon(
                          onPressed: _salvando ? null : _salvarConfiguracoes,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF7C6FE0),
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(vertical: 16),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                          icon: _salvando
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                                )
                              : const Icon(Icons.save_outlined),
                          label: Text(
                            _salvando ? 'Salvando...' : 'Salvar Configurações SaaS',
                            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                          ),
                        ),
                        const SizedBox(height: 24),
                      ],
                    ),
                  ),
                ),
      ),
    );
  }
}

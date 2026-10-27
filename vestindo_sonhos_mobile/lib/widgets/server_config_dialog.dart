import 'package:flutter/material.dart';
import '../services/api_service.dart';

/// Modal para configurar o endereço IP/URL do servidor da ONG dinamicamente.
/// Permite alternar entre Wi-Fi, Cabo USB e Emulador sem recompilar o app.
class ServerConfigDialog extends StatefulWidget {
  const ServerConfigDialog({super.key});

  static Future<bool?> show(BuildContext context) {
    return showDialog<bool>(
      context: context,
      barrierDismissible: true,
      builder: (context) => const ServerConfigDialog(),
    );
  }

  @override
  State<ServerConfigDialog> createState() => _ServerConfigDialogState();
}

class _ServerConfigDialogState extends State<ServerConfigDialog> {
  final _urlController = TextEditingController();
  bool _carregando = true;
  bool _testando = false;
  String? _resultadoTeste;
  bool _sucessoTeste = false;

  @override
  void initState() {
    super.initState();
    _carregarUrlAtual();
  }

  @override
  void dispose() {
    _urlController.dispose();
    super.dispose();
  }

  Future<void> _carregarUrlAtual() async {
    final url = await ApiService.getServerUrl();
    if (mounted) {
      setState(() {
        _urlController.text = url;
        _carregando = false;
      });
    }
  }

  Future<void> _testarConexao() async {
    final url = _urlController.text.trim();
    if (url.isEmpty) {
      setState(() {
        _resultadoTeste = 'Por favor, informe a URL ou IP do servidor.';
        _sucessoTeste = false;
      });
      return;
    }

    FocusScope.of(context).unfocus();

    setState(() {
      _testando = true;
      _resultadoTeste = null;
    });

    final ok = await ApiService.testConnection(url);

    if (mounted) {
      setState(() {
        _testando = false;
        _sucessoTeste = ok;
        _resultadoTeste = ok
            ? 'Conectado com sucesso! O servidor backend está respondendo.'
            : 'Falha na conexão. Verifique se o sistema está aberto no PC e no mesmo Wi-Fi.';
      });
    }
  }

  Future<void> _salvar() async {
    final url = _urlController.text.trim();
    if (url.isEmpty) return;

    await ApiService.setServerUrl(url);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Servidor configurado para: $url'),
          backgroundColor: const Color(0xFF7C6FE0),
          behavior: SnackBarBehavior.floating,
        ),
      );
      Navigator.of(context).pop(true);
    }
  }

  void _aplicarPredefinicao(String url) {
    setState(() {
      _urlController.text = url;
      _resultadoTeste = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 460),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(22),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Cabeçalho
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: const Color(0xFF7C6FE0).withValues(alpha: 0.12),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.dns_rounded, color: Color(0xFF7C6FE0), size: 26),
                  ),
                  const SizedBox(width: 14),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Servidor Backend',
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                        ),
                        Text(
                          'Endereço de rede da API',
                          style: TextStyle(fontSize: 12, color: Colors.grey),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 18),

              if (_carregando)
                const Center(
                  child: Padding(
                    padding: EdgeInsets.all(24),
                    child: CircularProgressIndicator(),
                  ),
                )
              else ...[
                // Campo de Endereço
                TextField(
                  controller: _urlController,
                  keyboardType: TextInputType.url,
                  decoration: InputDecoration(
                    labelText: 'URL ou IP do Computador',
                    hintText: 'http://192.168.1.50:5000',
                    prefixIcon: const Icon(Icons.wifi, size: 20),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                  ),
                ),
                const SizedBox(height: 12),

                // Atalhos Rápidos
                Wrap(
                  spacing: 6,
                  runSpacing: 4,
                  children: [
                    ActionChip(
                      avatar: const Icon(Icons.cable, size: 16),
                      label: const Text('Cabo USB', style: TextStyle(fontSize: 12)),
                      onPressed: () => _aplicarPredefinicao('http://localhost:5000'),
                    ),
                    ActionChip(
                      avatar: const Icon(Icons.android, size: 16),
                      label: const Text('Emulador', style: TextStyle(fontSize: 12)),
                      onPressed: () => _aplicarPredefinicao('http://10.0.2.2:5000'),
                    ),
                  ],
                ),
                const SizedBox(height: 14),

                // Botão Testar Conexão
                OutlinedButton.icon(
                  onPressed: _testando ? null : _testarConexao,
                  icon: _testando
                      ? const SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Icon(Icons.network_check_rounded, size: 18),
                  label: Text(_testando ? 'Testando conexão...' : 'Testar Comunicação'),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                ),

                // Feedback visual do teste
                if (_resultadoTeste != null) ...[
                  const SizedBox(height: 12),
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: _sucessoTeste ? Colors.green.shade50 : Colors.red.shade50,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(
                        color: _sucessoTeste ? Colors.green.shade300 : Colors.red.shade300,
                      ),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Icon(
                          _sucessoTeste ? Icons.check_circle : Icons.error_outline,
                          color: _sucessoTeste ? Colors.green.shade700 : Colors.red.shade700,
                          size: 20,
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            _resultadoTeste!,
                            style: TextStyle(
                              fontSize: 12,
                              color: _sucessoTeste ? Colors.green.shade900 : Colors.red.shade900,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],

                const SizedBox(height: 14),

                // Card explicativo de como achar o IP
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Theme.of(context).brightness == Brightness.dark
                        ? const Color(0xFF262A3D)
                        : Colors.grey.shade100,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(Icons.lightbulb_outline, size: 16, color: Colors.amber.shade800),
                          const SizedBox(width: 6),
                          const Text(
                            'Como descobrir o IP do seu PC:',
                            style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        '1. No PC, abra o Prompt (CMD) e digite: ipconfig\n'
                        '2. Copie o "Endereço IPv4" (ex: 192.168.1.15)\n'
                        '3. Digite acima: http://192.168.1.15:5000\n'
                        '4. Celular e PC devem estar no mesmo Wi-Fi!',
                        style: TextStyle(
                          fontSize: 11,
                          color: Theme.of(context).brightness == Brightness.dark
                              ? const Color(0xFFD1D5DB)
                              : Colors.black87,
                          height: 1.4,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 18),

                // Ações do Dialog
                Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    TextButton(
                      onPressed: () => Navigator.of(context).pop(false),
                      child: const Text('Cancelar'),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: _salvar,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF7C6FE0),
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: const Text('Salvar'),
                    ),
                  ],
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

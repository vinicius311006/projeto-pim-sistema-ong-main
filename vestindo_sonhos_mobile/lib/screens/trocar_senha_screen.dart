import 'package:flutter/material.dart';
import '../services/api_service.dart';

/// Tela para troca obrigatória de senha temporária/padrão no primeiro acesso via Mobile.
class TrocarSenhaScreen extends StatefulWidget {
  final int usuarioId;
  final String nome;

  const TrocarSenhaScreen({
    super.key,
    required this.usuarioId,
    required this.nome,
  });

  @override
  State<TrocarSenhaScreen> createState() => _TrocarSenhaScreenState();
}

class _TrocarSenhaScreenState extends State<TrocarSenhaScreen> {
  final _formKey = GlobalKey<FormState>();
  final _novaSenhaController = TextEditingController();
  final _confirmarSenhaController = TextEditingController();
  final _apiService = ApiService();

  bool _isLoading = false;
  bool _obscureNova = true;
  bool _obscureConfirmar = true;

  @override
  void dispose() {
    _novaSenhaController.dispose();
    _confirmarSenhaController.dispose();
    super.dispose();
  }

  Future<void> _salvarNovaSenha() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    FocusScope.of(context).unfocus();

    setState(() {
      _isLoading = true;
    });

    try {
      final novaSenha = _novaSenhaController.text;
      await _apiService.trocarSenhaObrigatoria(widget.usuarioId, novaSenha);

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Senha alterada com sucesso! Bem-vindo(a)!'),
          backgroundColor: Colors.teal.shade700,
          behavior: SnackBarBehavior.floating,
        ),
      );

      // Redireciona para o Home do App
      Navigator.pushReplacementNamed(context, '/home');
    } catch (e) {
      if (!mounted) return;

      final msg = e.toString().replaceFirst('Exception: ', '');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(msg),
          backgroundColor: Colors.red.shade700,
          behavior: SnackBarBehavior.floating,
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  Future<void> _cancelarESair() async {
    await _apiService.clearAuthData();
    if (!mounted) return;
    Navigator.pushReplacementNamed(context, '/login');
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Definir Nova Senha'),
        automaticallyImplyLeading: false,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded),
            tooltip: 'Sair',
            onPressed: _cancelarESair,
          ),
        ],
      ),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 20.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Ícone de Segurança
                  Center(
                    child: Container(
                      width: 86,
                      height: 86,
                      decoration: BoxDecoration(
                        color: theme.colorScheme.primary.withValues(alpha: 0.12),
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.lock_reset_rounded,
                        size: 46,
                        color: theme.colorScheme.primary,
                      ),
                    ),
                  ),
                  const SizedBox(height: 18),

                  // Título e Boas-Vindas
                  Text(
                    'Olá, ${widget.nome}!',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.brightness == Brightness.dark
                          ? const Color(0xFFF3F4F6)
                          : const Color(0xFF2C2C35),
                    ),
                  ),
                  const SizedBox(height: 8),

                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFFFBEB),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: const Color(0xFFFDE68A)),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Icon(
                          Icons.security_rounded,
                          size: 20,
                          color: Color(0xFFD97706),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            'Por medidas de segurança, como você está acessando com uma senha inicial ou padrão, é obrigatório cadastrar uma nova senha pessoal para continuar.',
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: const Color(0xFF92400E),
                              height: 1.4,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Campo Nova Senha
                  TextFormField(
                    controller: _novaSenhaController,
                    obscureText: _obscureNova,
                    textInputAction: TextInputAction.next,
                    decoration: InputDecoration(
                      labelText: 'Nova Senha Pessoal',
                      hintText: 'Mínimo de 6 caracteres',
                      prefixIcon: const Icon(Icons.lock_outline),
                      suffixIcon: IconButton(
                        icon: Icon(
                          _obscureNova
                              ? Icons.visibility_outlined
                              : Icons.visibility_off_outlined,
                        ),
                        onPressed: () {
                          setState(() {
                            _obscureNova = !_obscureNova;
                          });
                        },
                      ),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    validator: (val) {
                      if (val == null || val.isEmpty) {
                        return 'Informe a nova senha';
                      }
                      if (val.length < 6) {
                        return 'A senha deve ter ao menos 6 caracteres';
                      }
                      if (val == '123456' || val.toLowerCase() == 'ong@2026') {
                        return 'Não reutilize a senha padrão inicial';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Campo Confirmar Nova Senha
                  TextFormField(
                    controller: _confirmarSenhaController,
                    obscureText: _obscureConfirmar,
                    textInputAction: TextInputAction.done,
                    onFieldSubmitted: (_) => _salvarNovaSenha(),
                    decoration: InputDecoration(
                      labelText: 'Confirmar Nova Senha',
                      hintText: 'Repita a mesma senha',
                      prefixIcon: const Icon(Icons.lock_clock_outlined),
                      suffixIcon: IconButton(
                        icon: Icon(
                          _obscureConfirmar
                              ? Icons.visibility_outlined
                              : Icons.visibility_off_outlined,
                        ),
                        onPressed: () {
                          setState(() {
                            _obscureConfirmar = !_obscureConfirmar;
                          });
                        },
                      ),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    validator: (val) {
                      if (val == null || val.isEmpty) {
                        return 'Confirme a nova senha';
                      }
                      if (val != _novaSenhaController.text) {
                        return 'As senhas digitadas não conferem';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 24),

                  // Botão Salvar
                  ElevatedButton.icon(
                    onPressed: _isLoading ? null : _salvarNovaSenha,
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 15),
                      backgroundColor: theme.colorScheme.primary,
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      elevation: 2,
                    ),
                    icon: _isLoading
                        ? const SizedBox.shrink()
                        : const Icon(Icons.check_circle_outline),
                    label: _isLoading
                        ? const SizedBox(
                            height: 22,
                            width: 22,
                            child: CircularProgressIndicator(
                              strokeWidth: 2.5,
                              valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                            ),
                          )
                        : const Text(
                            'Salvar Nova Senha e Entrar',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                  ),

                  const SizedBox(height: 16),

                  // Botão Cancelar / Sair
                  TextButton.icon(
                    onPressed: _isLoading ? null : _cancelarESair,
                    icon: const Icon(Icons.arrow_back, size: 18),
                    label: const Text('Voltar para a tela de login'),
                    style: TextButton.styleFrom(
                      foregroundColor: Colors.grey.shade700,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

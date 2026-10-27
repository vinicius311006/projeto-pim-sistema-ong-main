import 'package:flutter/material.dart';
import '../main.dart';
import '../services/api_service.dart';
import '../widgets/server_config_dialog.dart';
import 'trocar_senha_screen.dart';

/// Tela de Login para Voluntários da ONG Vestindo Sonhos.
/// Autentica com e-mail e senha, salva o token JWT localmente e redireciona para a Home.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _senhaController = TextEditingController();
  final _apiService = ApiService();

  bool _isLoading = false;
  bool _obscureSenha = true;

  @override
  void dispose() {
    _emailController.dispose();
    _senhaController.dispose();
    super.dispose();
  }

  Future<void> _fazerLogin() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    // Fecha o teclado
    FocusScope.of(context).unfocus();

    setState(() {
      _isLoading = true;
    });

    try {
      final email = _emailController.text.trim();
      final senha = _senhaController.text;

      // Chama a API REST backend
      final resultado = await _apiService.login(email, senha);

      final token = resultado['token'] as String;
      final usuario = resultado['usuario'] as Map<String, dynamic>?;

      // Salva o token JWT no SharedPreferences
      await _apiService.saveToken(token);

      if (usuario != null) {
        await _apiService.saveUserData(
          nome: usuario['nome']?.toString() ?? '',
          email: usuario['email']?.toString() ?? '',
          cargo: usuario['cargo']?.toString() ?? 'Voluntário',
        );
      }

      if (!mounted) return;

      // Verifica se o usuário precisa obrigatoriamente trocar a senha temporária/padrão
      final bool exigeTroca = (usuario?['exigirTrocaSenha'] == true) ||
          (usuario?['ExigirTrocaSenha'] == true) ||
          (usuario?['exigirTrocaSenha']?.toString().toLowerCase() == 'true');

      if (exigeTroca) {
        final rawId = usuario?['id'] ?? usuario?['Id'];
        final usuarioId = rawId is int ? rawId : (int.tryParse(rawId?.toString() ?? '') ?? 0);
        final nomeUsuario = usuario?['nome']?.toString() ?? usuario?['Nome']?.toString() ?? 'Voluntário';

        Navigator.pushReplacement(
          context,
          MaterialPageRoute(
            builder: (_) => TrocarSenhaScreen(
              usuarioId: usuarioId,
              nome: nomeUsuario,
            ),
          ),
        );
        return;
      }

      // Exibe mensagem de boas-vindas
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Bem-vindo(a), ${usuario?['nome'] ?? 'Voluntário'}!'),
          backgroundColor: Colors.teal.shade700,
          behavior: SnackBarBehavior.floating,
        ),
      );

      // Navega para a HomeScreen substituindo a tela de login na pilha de navegação
      Navigator.pushReplacementNamed(context, '/home');
    } catch (e) {
      if (!mounted) return;

      final mensagemErro = e.toString().replaceFirst('Exception: ', '');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(mensagemErro),
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

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        actions: [
          IconButton(
            icon: Icon(
              isDark ? Icons.light_mode_rounded : Icons.dark_mode_outlined,
              color: const Color(0xFF7C6FE0),
            ),
            tooltip: isDark ? 'Modo Claro' : 'Modo Escuro',
            onPressed: toggleAppTheme,
          ),
          IconButton(
            icon: const Icon(Icons.settings_outlined, color: Color(0xFF7C6FE0)),
            tooltip: 'Configurar Servidor / IP',
            onPressed: () => ServerConfigDialog.show(context),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 28.0, vertical: 24.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Logo Oficial da ONG Vestindo Sonhos
                  Center(
                    child: Container(
                      width: 112,
                      height: 112,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        boxShadow: [
                          BoxShadow(
                            color: theme.colorScheme.primary.withValues(alpha: 0.28),
                            blurRadius: 16,
                            offset: const Offset(0, 6),
                          ),
                        ],
                      ),
                      child: ClipOval(
                        child: Image.asset(
                          'assets/images/logo.png',
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => Container(
                            color: theme.colorScheme.primary.withValues(alpha: 0.12),
                            child: Icon(
                              Icons.volunteer_activism_rounded,
                              size: 54,
                              color: theme.colorScheme.primary,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),

                  // Título e Subtítulo
                  Text(
                    'Vestindo Sonhos',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.primary,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    'Portal do Voluntário',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: Colors.grey.shade600,
                    ),
                  ),
                  const SizedBox(height: 36),

                  // Campo E-mail
                  TextFormField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    decoration: InputDecoration(
                      labelText: 'E-mail',
                      hintText: 'exemplo@vestindosonhos.org',
                      prefixIcon: const Icon(Icons.email_outlined),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) {
                        return 'Informe o seu e-mail';
                      }
                      if (!value.contains('@') || !value.contains('.')) {
                        return 'Informe um e-mail válido';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 18),

                  // Campo Senha
                  TextFormField(
                    controller: _senhaController,
                    obscureText: _obscureSenha,
                    textInputAction: TextInputAction.done,
                    onFieldSubmitted: (_) => _fazerLogin(),
                    decoration: InputDecoration(
                      labelText: 'Senha',
                      prefixIcon: const Icon(Icons.lock_outline),
                      suffixIcon: IconButton(
                        icon: Icon(
                          _obscureSenha ? Icons.visibility_outlined : Icons.visibility_off_outlined,
                        ),
                        onPressed: () {
                          setState(() {
                            _obscureSenha = !_obscureSenha;
                          });
                        },
                      ),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Informe a sua senha';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 28),

                  // Botão Entrar
                  ElevatedButton(
                    onPressed: _isLoading ? null : _fazerLogin,
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      backgroundColor: theme.colorScheme.primary,
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      elevation: 2,
                    ),
                    child: _isLoading
                        ? const SizedBox(
                            height: 22,
                            width: 22,
                            child: CircularProgressIndicator(
                              strokeWidth: 2.5,
                              valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                            ),
                          )
                        : const Text(
                            'Entrar',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                  ),

                  const SizedBox(height: 24),
                  Text(
                    'Juntos transformando vidas através da solidariedade.',
                    textAlign: TextAlign.center,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: Colors.grey.shade500,
                      fontStyle: FontStyle.italic,
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

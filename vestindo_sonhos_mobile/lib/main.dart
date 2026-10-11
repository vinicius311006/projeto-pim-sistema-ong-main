import 'package:flutter/material.dart';
import 'screens/home_screen.dart';
import 'screens/login_screen.dart';
import 'screens/eventos_screen.dart';
import 'services/api_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Verifica se o voluntário já possui uma sessão ativa salva localmente
  final apiService = ApiService();
  final token = await apiService.getToken();
  final usuarioJaLogado = token != null && token.isNotEmpty;

  runApp(VestindoSonhosApp(inicialmenteLogado: usuarioJaLogado));
}

/// Aplicativo Principal da ONG Vestindo Sonhos
class VestindoSonhosApp extends StatelessWidget {
  final bool inicialmenteLogado;

  const VestindoSonhosApp({super.key, required this.inicialmenteLogado});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Vestindo Sonhos',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF00796B), // Tom Teal sóbrio e humanitário
          primary: const Color(0xFF00796B),
          secondary: const Color(0xFF26A69A),
        ),
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
      ),
      initialRoute: inicialmenteLogado ? '/home' : '/login',
      routes: {
        '/login': (context) => const LoginScreen(),
        '/home': (context) => const HomeScreen(),
        '/eventos': (context) => const EventosScreen(),
      },
    );
  }
}

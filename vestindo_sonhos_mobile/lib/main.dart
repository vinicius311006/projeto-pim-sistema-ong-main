import 'package:flutter/material.dart';
import 'screens/home_screen.dart';
import 'screens/login_screen.dart';
import 'screens/eventos_screen.dart';
import 'screens/inventario_screen.dart';
import 'screens/beneficiarios_screen.dart';
import 'screens/financeiro_screen.dart';
import 'services/api_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Verifica se o usuário já possui uma sessão ativa salva localmente
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
    const primaryColor = Color(0xFF7C6FE0);
    const secondaryColor = Color(0xFF9B8DE3);
    const backgroundColor = Color(0xFFF3F4FA);

    return MaterialApp(
      title: 'Vestindo Sonhos',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: backgroundColor,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryColor,
          primary: primaryColor,
          secondary: secondaryColor,
          surface: Colors.white,
        ),
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
          backgroundColor: primaryColor,
          foregroundColor: Colors.white,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: primaryColor,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
        ),
      ),
      initialRoute: inicialmenteLogado ? '/home' : '/login',
      routes: {
        '/login': (context) => const LoginScreen(),
        '/home': (context) => const HomeScreen(),
        '/eventos': (context) => const EventosScreen(),
        '/inventario': (context) => const InventarioScreen(),
        '/beneficiarios': (context) => const BeneficiariosScreen(),
        '/financeiro': (context) => const FinanceiroScreen(),
      },
    );
  }
}

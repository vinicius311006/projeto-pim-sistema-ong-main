import 'package:flutter/material.dart';
import 'screens/login_screen.dart';
import 'screens/home_screen.dart';
import 'screens/eventos_screen.dart';
import 'screens/beneficiarios_screen.dart';
import 'screens/inventario_screen.dart';
import 'screens/financeiro_screen.dart';
import 'screens/configuracoes_saas_screen.dart';
import 'services/api_service.dart';

/// Notificador global para alternar entre Modo Claro e Modo Escuro em tempo real.
final ValueNotifier<ThemeMode> appThemeNotifier = ValueNotifier<ThemeMode>(ThemeMode.light);

Future<void> toggleAppTheme() async {
  final isCurrentlyDark = appThemeNotifier.value == ThemeMode.dark;
  final nextMode = isCurrentlyDark ? ThemeMode.light : ThemeMode.dark;
  appThemeNotifier.value = nextMode;
  await ApiService.setDarkMode(!isCurrentlyDark);
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final isDark = await ApiService.getDarkMode();
  appThemeNotifier.value = isDark ? ThemeMode.dark : ThemeMode.light;
  runApp(const VestindoSonhosApp());
}

/// Aplicativo Móvel da ONG Vestindo Sonhos.
/// Permite que voluntários e administradores acompanhem o Dashboard executivo,
/// eventos, inventário, beneficiários, financeiro e configurações SaaS.
class VestindoSonhosApp extends StatelessWidget {
  const VestindoSonhosApp({super.key});

  @override
  Widget build(BuildContext context) {
    const primaryLilac = Color(0xFF7C6FE0);
    const secondaryCyan = Color(0xFF00B4D8);

    return ValueListenableBuilder<ThemeMode>(
      valueListenable: appThemeNotifier,
      builder: (context, currentMode, _) {
        return MaterialApp(
          title: 'Vestindo Sonhos - ONG',
          debugShowCheckedModeBanner: false,
          themeMode: currentMode,
          theme: ThemeData(
            useMaterial3: true,
            brightness: Brightness.light,
            colorScheme: ColorScheme.fromSeed(
              seedColor: primaryLilac,
              brightness: Brightness.light,
              primary: primaryLilac,
              secondary: secondaryCyan,
              surface: Colors.white,
            ),
            scaffoldBackgroundColor: const Color(0xFFF3F4FA),
            cardColor: Colors.white,
            appBarTheme: const AppBarTheme(
              backgroundColor: primaryLilac,
              foregroundColor: Colors.white,
              elevation: 0,
              centerTitle: true,
            ),
            cardTheme: CardThemeData(
              color: Colors.white,
              elevation: 1.5,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
            ),
            inputDecorationTheme: InputDecorationTheme(
              filled: true,
              fillColor: Colors.white,
              contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),
          darkTheme: ThemeData(
            useMaterial3: true,
            brightness: Brightness.dark,
            colorScheme: ColorScheme.fromSeed(
              seedColor: primaryLilac,
              brightness: Brightness.dark,
              primary: primaryLilac,
              secondary: secondaryCyan,
              surface: const Color(0xFF1F2232),
            ),
            scaffoldBackgroundColor: const Color(0xFF151722),
            cardColor: const Color(0xFF1F2232),
            appBarTheme: const AppBarTheme(
              backgroundColor: Color(0xFF1F2232),
              foregroundColor: Colors.white,
              elevation: 0,
              centerTitle: true,
            ),
            drawerTheme: const DrawerThemeData(
              backgroundColor: Color(0xFF1A1D2B),
            ),
            dialogTheme: DialogThemeData(
              backgroundColor: const Color(0xFF1F2232),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            ),
            cardTheme: CardThemeData(
              color: const Color(0xFF1F2232),
              elevation: 2,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
            ),
            inputDecorationTheme: InputDecorationTheme(
              filled: true,
              fillColor: const Color(0xFF262A3D),
              contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),
          home: const AuthCheckScreen(),
          routes: {
            '/login': (context) => const LoginScreen(),
            '/home': (context) => const HomeScreen(),
            '/eventos': (context) => const EventosScreen(),
            '/beneficiarios': (context) => const BeneficiariosScreen(),
            '/inventario': (context) => const InventarioScreen(),
            '/financeiro': (context) => const FinanceiroScreen(),
            '/configuracoes': (context) => const ConfiguracoesSaasScreen(),
          },
        );
      },
    );
  }
}

/// Widget inicial que verifica se já existe um token JWT salvo no dispositivo.
/// Se existir, redireciona diretamente para o Dashboard (HomeScreen). Caso contrário, abre a LoginScreen.
class AuthCheckScreen extends StatefulWidget {
  const AuthCheckScreen({super.key});

  @override
  State<AuthCheckScreen> createState() => _AuthCheckScreenState();
}

class _AuthCheckScreenState extends State<AuthCheckScreen> {
  final _apiService = ApiService();

  @override
  void initState() {
    super.initState();
    _verificarAutenticacao();
  }

  Future<void> _verificarAutenticacao() async {
    final token = await _apiService.getToken();

    if (!mounted) return;

    if (token != null && token.isNotEmpty) {
      Navigator.pushReplacementNamed(context, '/home');
    } else {
      Navigator.pushReplacementNamed(context, '/login');
    }
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: CircularProgressIndicator(),
      ),
    );
  }
}

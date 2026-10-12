import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/evento_model.dart';
import '../models/dashboard_model.dart';
import '../models/beneficiario_model.dart';
import '../models/inventario_model.dart';
import '../models/financeiro_model.dart';

/// Serviço responsável pela integração HTTP com o backend ASP.NET Core da ONG Vestindo Sonhos.
class ApiService {
  /// URL base da API REST.
  static String get baseUrl {
    try {
      if (Platform.isAndroid) {
        return 'http://10.0.2.2:5000/api';
      }
    } catch (_) {}
    return 'http://localhost:5000/api';
  }

  static const String _keyToken = 'jwt_auth_token';
  static const String _keyUserNome = 'user_nome';
  static const String _keyUserEmail = 'user_email';
  static const String _keyUserCargo = 'user_cargo';

  /// Realiza o login na API REST.
  /// Endpoint: POST /api/auth/login
  Future<Map<String, dynamic>> login(String email, String password) async {
    final url = Uri.parse('$baseUrl/auth/login');

    try {
      final response = await http
          .post(
            url,
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
            },
            body: jsonEncode({
              'email': email.trim(),
              'senha': password,
            }),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as Map<String, dynamic>;
        return data;
      } else if (response.statusCode == 401) {
        final data = jsonDecode(response.body);
        final mensagem = data['mensagem'] ?? 'E-mail ou senha incorretos.';
        throw Exception(mensagem);
      } else if (response.statusCode == 400) {
        throw Exception('Dados de login inválidos. Verifique os campos informados.');
      } else {
        throw Exception('Erro no servidor (${response.statusCode}). Tente novamente mais tarde.');
      }
    } on SocketException {
      throw Exception('Não foi possível conectar ao servidor. Verifique sua conexão ou se a API está em execução.');
    } on http.ClientException {
      throw Exception('Falha na comunicação de rede com o servidor.');
    } catch (e) {
      if (e is Exception) rethrow;
      throw Exception('Erro inesperado durante a autenticação: $e');
    }
  }

  /// Obtém dados consolidados do Dashboard.
  /// Endpoint: GET /api/dashboard
  Future<DashboardModel> getDashboard(String token) async {
    final url = Uri.parse('$baseUrl/dashboard');
    final response = await http.get(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      return DashboardModel.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception('Sessão expirada. Faça login novamente.');
    } else {
      throw Exception('Falha ao carregar dashboard (${response.statusCode}).');
    }
  }

  /// Obtém a lista de eventos ativos.
  /// Endpoint: GET /api/eventos
  Future<List<EventoModel>> getEventos(String token) async {
    final url = Uri.parse('$baseUrl/eventos');

    try {
      final response = await http
          .get(
            url,
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
              'Authorization': 'Bearer $token',
            },
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final List<dynamic> listaJson = jsonDecode(response.body);
        return listaJson.map((item) => EventoModel.fromJson(item as Map<String, dynamic>)).toList();
      } else if (response.statusCode == 401) {
        throw Exception('Sessão expirada. Por favor, realize login novamente.');
      } else if (response.statusCode == 403) {
        throw Exception('Acesso não autorizado para este perfil.');
      } else {
        throw Exception('Falha ao carregar eventos (${response.statusCode}).');
      }
    } on SocketException {
      throw Exception('Não foi possível conectar ao servidor backend.');
    } catch (e) {
      if (e is Exception) rethrow;
      throw Exception('Erro ao buscar eventos: $e');
    }
  }

  /// Obtém a lista de beneficiários atendidos.
  /// Endpoint: GET /api/beneficiarios
  Future<List<BeneficiarioModel>> getBeneficiarios(String token) async {
    final url = Uri.parse('$baseUrl/beneficiarios');
    final response = await http.get(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      final List<dynamic> lista = jsonDecode(response.body);
      return lista.map((item) => BeneficiarioModel.fromJson(item as Map<String, dynamic>)).toList();
    } else if (response.statusCode == 401) {
      throw Exception('Sessão expirada. Faça login novamente.');
    } else {
      throw Exception('Falha ao carregar beneficiários (${response.statusCode}).');
    }
  }

  /// Obtém a lista de itens e quantidades do inventário/estoque.
  /// Endpoint: GET /api/inventario
  Future<List<InventarioModel>> getInventario(String token) async {
    final url = Uri.parse('$baseUrl/inventario');
    final response = await http.get(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      final List<dynamic> lista = jsonDecode(response.body);
      return lista.map((item) => InventarioModel.fromJson(item as Map<String, dynamic>)).toList();
    } else if (response.statusCode == 401) {
      throw Exception('Sessão expirada. Faça login novamente.');
    } else {
      throw Exception('Falha ao carregar inventário (${response.statusCode}).');
    }
  }

  /// Obtém o resumo financeiro com carteiras e movimentações.
  /// Endpoint: GET /api/financeiro
  Future<FinanceiroModel> getFinanceiro(String token) async {
    final url = Uri.parse('$baseUrl/financeiro');
    final response = await http.get(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      return FinanceiroModel.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception('Sessão expirada. Faça login novamente.');
    } else {
      throw Exception('Falha ao carregar dados financeiros (${response.statusCode}).');
    }
  }

  // ==========================================
  // Métodos Auxiliares de Persistência Local
  // ==========================================

  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyToken, token);
  }

  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyToken);
  }

  Future<void> saveUserData({
    required String nome,
    required String email,
    required String cargo,
  }) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyUserNome, nome);
    await prefs.setString(_keyUserEmail, email);
    await prefs.setString(_keyUserCargo, cargo);
  }

  Future<String?> getUserNome() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyUserNome);
  }

  Future<String?> getUserEmail() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyUserEmail);
  }

  Future<String?> getUserCargo() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyUserCargo);
  }

  Future<void> clearAuthData() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyToken);
    await prefs.remove(_keyUserNome);
    await prefs.remove(_keyUserEmail);
    await prefs.remove(_keyUserCargo);
  }
}

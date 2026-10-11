import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/evento_model.dart';

/// Serviço responsável pela integração HTTP com o backend ASP.NET Core da ONG Vestindo Sonhos.
class ApiService {
  /// URL base da API REST.
  /// - Para Emulador Android: utilize 'http://10.0.2.2:5000/api' ou a porta configurada no backend.
  /// - Para Dispositivo Físico: utilize o IP local da máquina na mesma rede Wi-Fi (ex: 'http://192.168.1.100:5000/api').
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

  /// Realiza o login do voluntário/usuário na API REST.
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

  /// Obtém a lista de eventos ativos a partir da API REST.
  /// Endpoint: GET /api/eventos
  /// Requer token JWT no cabeçalho Authorization: Bearer <token>
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

  // ==========================================
  // Métodos Auxiliares de Persistência Local
  // ==========================================

  /// Salva o token JWT de autenticação no SharedPreferences.
  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyToken, token);
  }

  /// Recupera o token JWT salvo localmente.
  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyToken);
  }

  /// Salva informações resumidas do usuário logado.
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

  /// Recupera o nome do usuário salvo.
  Future<String?> getUserNome() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyUserNome);
  }

  /// Remove o token e os dados locais ao realizar logout.
  Future<void> clearAuthData() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyToken);
    await prefs.remove(_keyUserNome);
    await prefs.remove(_keyUserEmail);
    await prefs.remove(_keyUserCargo);
  }
}

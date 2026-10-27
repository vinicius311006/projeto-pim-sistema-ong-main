import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/evento_model.dart';
import '../models/dashboard_model.dart';
import '../models/beneficiario_model.dart';
import '../models/inventario_model.dart';
import '../models/financeiro_model.dart';
import '../models/voluntario_model.dart';

/// Serviço responsável pela integração HTTP com o backend ASP.NET Core da ONG Vestindo Sonhos.
class ApiService {
  static const String _keyServerUrl = 'custom_server_url';
  static String? _cachedServerUrl;

  /// Retorna a URL base do servidor por padrão dependendo da plataforma
  static String getDefaultServerUrl() {
    try {
      if (Platform.isAndroid) {
        return 'http://10.0.2.2:5000';
      }
    } catch (_) {}
    return 'http://localhost:5000';
  }

  /// Obtém a URL do servidor configurada ou padrão
  static Future<String> getServerUrl() async {
    if (_cachedServerUrl != null && _cachedServerUrl!.isNotEmpty) {
      return _cachedServerUrl!;
    }
    try {
      final prefs = await SharedPreferences.getInstance();
      final saved = prefs.getString(_keyServerUrl);
      if (saved != null && saved.trim().isNotEmpty) {
        _cachedServerUrl = saved.trim();
        return _cachedServerUrl!;
      }
    } catch (_) {}
    _cachedServerUrl = getDefaultServerUrl();
    return _cachedServerUrl!;
  }

  /// Salva uma nova URL/IP de servidor (remove barras finais ou /api se houver)
  static Future<void> setServerUrl(String url) async {
    var clean = url.trim();
    if (clean.endsWith('/')) {
      clean = clean.substring(0, clean.length - 1);
    }
    if (clean.endsWith('/api')) {
      clean = clean.substring(0, clean.length - 4);
    }
    if (!clean.startsWith('http://') && !clean.startsWith('https://')) {
      clean = 'http://$clean';
    }
    _cachedServerUrl = clean;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyServerUrl, clean);
  }

  /// Retorna o endpoint base com sufixo /api
  static Future<String> getBaseUrl() async {
    final server = await getServerUrl();
    return '$server/api';
  }

  /// Getter síncrono para compatibilidade
  static String get baseUrl {
    final s = _cachedServerUrl ?? getDefaultServerUrl();
    return '$s/api';
  }

  /// Testa se o servidor backend está online e respondendo
  static Future<bool> testConnection(String url) async {
    try {
      var clean = url.trim();
      if (clean.endsWith('/')) {
        clean = clean.substring(0, clean.length - 1);
      }
      if (clean.endsWith('/api')) {
        clean = clean.substring(0, clean.length - 4);
      }
      if (!clean.startsWith('http://') && !clean.startsWith('https://')) {
        clean = 'http://$clean';
      }
      final testUri = Uri.parse('$clean/api/eventos');
      final response = await http.get(testUri).timeout(const Duration(seconds: 4));
      // Se responder qualquer código HTTP (mesmo 401 não autenticado), o servidor existe e está vivo!
      return response.statusCode >= 200 && response.statusCode < 500;
    } catch (_) {
      return false;
    }
  }

  static const String _keyToken = 'jwt_auth_token';
  static const String _keyUserNome = 'user_nome';
  static const String _keyUserEmail = 'user_email';
  static const String _keyUserCargo = 'user_cargo';

  /// Realiza o login na API REST.
  /// Endpoint: POST /api/auth/login
  Future<Map<String, dynamic>> login(String email, String password) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/auth/login');

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

  /// Realiza a troca obrigatória de senha temporária/padrão via API.
  /// Endpoint: POST /api/auth/trocar-senha-obrigatoria
  Future<void> trocarSenhaObrigatoria(int usuarioId, String novaSenha) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/auth/trocar-senha-obrigatoria');
    final token = await getToken();

    try {
      final response = await http
          .post(
            url,
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
              if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
            },
            body: jsonEncode({
              'usuarioId': usuarioId,
              'novaSenha': novaSenha,
            }),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return;
      } else {
        final data = jsonDecode(response.body);
        final mensagem = data['mensagem'] ?? 'Não foi possível alterar a senha. Tente novamente.';
        throw Exception(mensagem);
      }
    } on SocketException {
      throw Exception('Não foi possível conectar ao servidor. Verifique sua conexão.');
    } catch (e) {
      if (e is Exception) rethrow;
      throw Exception('Erro ao trocar a senha: $e');
    }
  }

  /// Obtém dados consolidados do Dashboard.
  /// Endpoint: GET /api/dashboard
  Future<DashboardModel> getDashboard(String token) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/dashboard');
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
    final base = await getBaseUrl();
    final url = Uri.parse('$base/eventos');

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
    final base = await getBaseUrl();
    final url = Uri.parse('$base/beneficiarios');
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
    final base = await getBaseUrl();
    final url = Uri.parse('$base/inventario');
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
    final base = await getBaseUrl();
    final url = Uri.parse('$base/financeiro');
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
  // Operações CRUD - Beneficiários
  // ==========================================

  Future<void> criarBeneficiario(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/beneficiarios');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao cadastrar beneficiário');
      throw Exception(msg);
    }
  }

  Future<void> editarBeneficiario(String token, int id, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/beneficiarios/$id');
    final response = await http.put(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao atualizar beneficiário');
      throw Exception(msg);
    }
  }

  Future<void> excluirBeneficiario(String token, int id) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/beneficiarios/$id');
    final response = await http.delete(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao excluir beneficiário');
      throw Exception(msg);
    }
  }

  // ==========================================
  // Operações CRUD - Voluntários
  // ==========================================

  Future<List<VoluntarioModel>> getVoluntarios(String token) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/voluntarios');
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
      return lista.map((e) => VoluntarioModel.fromJson(e as Map<String, dynamic>)).toList();
    } else if (response.statusCode == 401) {
      throw Exception('Sessão expirada. Faça login novamente.');
    } else {
      throw Exception('Falha ao carregar voluntários (${response.statusCode}).');
    }
  }

  Future<void> criarVoluntario(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/voluntarios');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao cadastrar voluntário');
      throw Exception(msg);
    }
  }

  Future<void> editarVoluntario(String token, int id, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/voluntarios/$id');
    final response = await http.put(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao atualizar voluntário');
      throw Exception(msg);
    }
  }

  Future<void> excluirVoluntario(String token, int id) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/voluntarios/$id');
    final response = await http.delete(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao excluir voluntário');
      throw Exception(msg);
    }
  }

  // ==========================================
  // Operações CRUD - Eventos
  // ==========================================

  Future<void> criarEvento(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/eventos');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao cadastrar evento');
      throw Exception(msg);
    }
  }

  Future<void> editarEvento(String token, int id, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/eventos/$id');
    final response = await http.put(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao atualizar evento');
      throw Exception(msg);
    }
  }

  Future<void> excluirEvento(String token, int id) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/eventos/$id');
    final response = await http.delete(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao excluir evento');
      throw Exception(msg);
    }
  }

  // ==========================================
  // Operações CRUD - Inventário & Produtos
  // ==========================================

  Future<List<Map<String, dynamic>>> getProdutosInventario(String token) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/inventario/produtos');
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
      return lista.cast<Map<String, dynamic>>();
    } else {
      throw Exception('Falha ao obter lista de produtos (${response.statusCode}).');
    }
  }

  Future<void> criarProduto(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/inventario/produtos');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao cadastrar produto');
      throw Exception(msg);
    }
  }

  Future<void> excluirProduto(String token, int id) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/inventario/produtos/$id');
    final response = await http.delete(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao excluir produto');
      throw Exception(msg);
    }
  }

  Future<void> registrarMovimentacaoInventario(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/inventario/movimentacao');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao registrar movimentação');
      throw Exception(msg);
    }
  }

  // ==========================================
  // Operações CRUD - Financeiro
  // ==========================================

  Future<void> criarTransacaoFinanceira(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/financeiro/transacao');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200 && response.statusCode != 201) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao registrar lançamento financeiro');
      throw Exception(msg);
    }
  }

  Future<void> excluirTransacaoFinanceira(String token, int id) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/financeiro/transacao/$id');
    final response = await http.delete(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao excluir lançamento financeiro');
      throw Exception(msg);
    }
  }

  static String _extrairMensagemErro(String responseBody, String padrao) {
    try {
      final decoded = jsonDecode(responseBody);
      if (decoded is Map<String, dynamic>) {
        if (decoded.containsKey('mensagem')) {
          return decoded['mensagem'].toString();
        }
        if (decoded.containsKey('title')) {
          return decoded['title'].toString();
        }
      }
    } catch (_) {}
    return padrao;
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

  // ==========================================
  // Configurações SaaS (Administrador) & Tema Dark Mode
  // ==========================================

  static const String _keyDarkMode = 'vs_mobile_dark_mode';

  static Future<bool> getDarkMode() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool(_keyDarkMode) ?? false;
  }

  static Future<void> setDarkMode(bool isDark) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyDarkMode, isDark);
  }

  Future<Map<String, dynamic>> getConfiguracoesSaas(String token) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/dashboard/configuracoes');
    final response = await http.get(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    } else if (response.statusCode == 403) {
      throw Exception('Acesso restrito a Administradores.');
    } else {
      throw Exception('Falha ao carregar configurações (${response.statusCode}).');
    }
  }

  Future<void> salvarConfiguracoesSaas(String token, Map<String, dynamic> dados) async {
    final base = await getBaseUrl();
    final url = Uri.parse('$base/dashboard/configuracoes');
    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode(dados),
    ).timeout(const Duration(seconds: 15));

    if (response.statusCode != 200) {
      final msg = _extrairMensagemErro(response.body, 'Falha ao salvar configurações SaaS');
      throw Exception(msg);
    }
  }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    /// <summary>
    /// Controlador REST para autenticação via JSON Web Token (JWT).
    /// Permite que clientes externos e o aplicativo mobile realizem login de forma segura.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            VestindosonhosdbContext context,
            IConfiguration configuration,
            ILogger<AuthApiController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Realiza a autenticação do usuário/voluntário e gera o token de acesso JWT.
        /// </summary>
        /// <param name="request">Credenciais de acesso (Email e Senha)</param>
        /// <returns>Token JWT e dados do usuário autenticado</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailLimpo = request.Email.Trim();

            // 1. Busca o usuário correspondente no banco de dados com seu Cargo
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == emailLimpo);

            // 2. Se não encontrar em Usuário, verifica se existe registro na tabela Voluntário para log informativo
            if (usuario == null)
            {
                var voluntarioExistente = await _context.Voluntario
                    .FirstOrDefaultAsync(v => v.Email == emailLimpo);

                if (voluntarioExistente != null)
                {
                    _logger.LogWarning("Tentativa de login de voluntário cadastrado sem conta de usuário: {Email}", emailLimpo);
                }

                return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
            }

            // 3. Validação de senha com suporte a senhas legadas e hash seguro (ASP.NET Identity PasswordHasher)
            var passwordHasher = new PasswordHasher<Usuario>();

            // Suporte à migração gradual de senhas legadas que ainda não utilizam hash PBKDF2
            if (!usuario.Senha.StartsWith("$"))
            {
                if (usuario.Senha == request.Senha)
                {
                    usuario.Senha = passwordHasher.HashPassword(usuario, request.Senha);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Senha do usuário {Email} atualizada com hash seguro com sucesso.", usuario.Email);
                }
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, request.Senha);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Falha na autenticação para o e-mail: {Email}", usuario.Email);
                return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
            }

            // 4. Busca se o usuário possui vínculo correspondente na tabela Voluntario (por Email ou CPF)
            var voluntario = await _context.Voluntario
                .FirstOrDefaultAsync(v => v.Email == usuario.Email || (!string.IsNullOrEmpty(usuario.Cpf) && v.Cpf == usuario.Cpf));

            // 5. Configuração das credenciais e claims do token JWT
            var jwtKey = _configuration["Jwt:Key"] ?? "VestindoSonhos_ChaveSecretaSuperSegura_JWT_2026_Key!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "VestindoSonhosAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "VestindoSonhosMobile";
            var expireHours = int.TryParse(_configuration["Jwt:ExpireHours"], out var hours) ? hours : 8;

            var expiration = DateTime.UtcNow.AddHours(expireHours);
            var cargo = await _context.Cargo.FindAsync(usuario.Cargo);
            var cargoNome = cargo?.Nome ?? (usuario.Cargo == 1 ? "Administrador" : "Colaborador");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, cargoNome),
                new Claim("UserId", usuario.Id.ToString()),
                new Claim("Cargo", cargoNome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (voluntario != null)
            {
                claims.Add(new Claim("VoluntarioId", voluntario.Id.ToString()));
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            _logger.LogInformation("Token JWT gerado com sucesso para {Email} (Perfil: {Cargo})", usuario.Email, cargoNome);

            return Ok(new LoginResponseDto
            {
                Token = tokenString,
                TokenType = "Bearer",
                Expiration = expiration,
                Usuario = new UsuarioInfoDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Cargo = cargoNome,
                    VoluntarioId = voluntario?.Id,
                    ExigirTrocaSenha = usuario.ExigirTrocaSenha
                }
            });
        }

        /// <summary>
        /// Endpoint para troca obrigatória de senha temporária/inicial via aplicativo móvel.
        /// </summary>
        [HttpPost("trocar-senha-obrigatoria")]
        public async Task<IActionResult> TrocarSenhaObrigatoria([FromBody] TrocarSenhaObrigatoriaApiRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 6)
            {
                return BadRequest(new { mensagem = "A nova senha deve ter no mínimo 6 caracteres." });
            }

            if (request.NovaSenha == "123456" || request.NovaSenha.Equals("Ong@2026", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { mensagem = "Por motivos de segurança, você não pode reutilizar a senha padrão inicial. Escolha uma nova senha pessoal." });
            }

            var usuario = await _context.Usuario.FindAsync(request.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Senha = passwordHasher.HashPassword(usuario, request.NovaSenha);
            usuario.ExigirTrocaSenha = false;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Senha atualizada via API mobile com sucesso para o usuário ID: {Id}", usuario.Id);
            return Ok(new { sucesso = true, mensagem = "Senha alterada com sucesso!" });
        }
    }
}

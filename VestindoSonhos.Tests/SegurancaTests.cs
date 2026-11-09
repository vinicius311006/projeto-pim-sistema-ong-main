using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Helpers;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;
using VestindoSonhos.Tests.Helpers;
using Xunit;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados de Segurança (Prevenção de IDOR, Controle de Acesso, Open Redirect e Criptografia JWT).
    /// </summary>
    public class SegurancaTests
    {
        [Fact(DisplayName = "1. TrocarSenhaObrigatoria sem autenticação deve retornar 401 Unauthorized")]
        public async Task TrocarSenhaObrigatoria_SemAutenticacao_DeveRetornar401()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var config = new ConfigurationBuilder().Build();
            var controller = new AuthApiController(context, config, NullLogger<AuthApiController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var request = new TrocarSenhaObrigatoriaApiRequestDto
            {
                UsuarioId = 1,
                NovaSenha = "NovaSenhaSegura@2026"
            };

            var resultado = await controller.TrocarSenhaObrigatoria(request);
            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact(DisplayName = "2. Tentativa de IDOR para trocar senha de outro usuário deve retornar 403 Forbidden")]
        public async Task TrocarSenhaObrigatoria_TentativaIdorOutroUsuario_DeveRetornar403()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var config = new ConfigurationBuilder().Build();
            var controller = new AuthApiController(context, config, NullLogger<AuthApiController>.Instance);

            // Usuário logado é ID 2 (Colaborador), mas tenta alterar a senha do ID 1 (Administrador)
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador", userId: 2);

            var request = new TrocarSenhaObrigatoriaApiRequestDto
            {
                UsuarioId = 1,
                NovaSenha = "NovaSenhaSegura@2026"
            };

            var resultado = await controller.TrocarSenhaObrigatoria(request);
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact(DisplayName = "3. TrocarSenhaObrigatoria para conta que não exige troca deve retornar 400 BadRequest")]
        public async Task TrocarSenhaObrigatoria_SemPendenciaDeTroca_DeveRetornar400()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Usuario.Add(new Usuario
            {
                Id = 5,
                Nome = "Admin Seguro",
                Email = "admin@ong.org",
                Senha = "hash",
                Telefone = "11999999999",
                Endereco = "Rua A",
                Cpf = "12345678901",
                Cargo = 1,
                ExigirTrocaSenha = false
            });
            await context.SaveChangesAsync();

            var config = new ConfigurationBuilder().Build();
            var controller = new AuthApiController(context, config, NullLogger<AuthApiController>.Instance);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador", userId: 5);

            var request = new TrocarSenhaObrigatoriaApiRequestDto
            {
                UsuarioId = 5,
                NovaSenha = "NovaSenhaSegura@2026"
            };

            var resultado = await controller.TrocarSenhaObrigatoria(request);
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact(DisplayName = "4. Prevenção de Open Redirect no cadastro de Categoria deve bloquear URLs externas")]
        public async Task CategoriaCriar_ComUrlExternaMaliciosa_NaoDeveRedirecionarParaFora()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new VestindoSonhos.Controllers.Categoria(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador", userId: 1);

            var novaCategoria = new Categoria { Nome = "Campanha Agasalho" };
            var resultado = await controller.Criar(novaCategoria, returnUrl: "https://site-malicioso.com/phishing");

            // Deve ignorar a URL externa e fazer RedirectToAction interno para Index
            var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact(DisplayName = "5. JwtKeyHelper deve gerar chave criptográfica forte mesmo sem configuração definida")]
        public void JwtKeyHelper_SemConfiguracao_DeveGerarChaveForteConsistente()
        {
            var config = new ConfigurationBuilder().Build();
            var chave1 = JwtKeyHelper.ObterChaveJwt(config);
            var chave2 = JwtKeyHelper.ObterChaveJwt(config);

            Assert.False(string.IsNullOrWhiteSpace(chave1));
            Assert.Equal(chave1, chave2);
            Assert.True(Convert.FromBase64String(chave1).Length >= 64);
        }
    }
}

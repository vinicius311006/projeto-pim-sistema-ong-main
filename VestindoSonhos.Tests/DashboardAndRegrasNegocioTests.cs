using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;
using VestindoSonhos.Tests.Helpers;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados de Regras de Negócio, Segurança (RBAC) e Dashboard.
    /// Valida o isolamento de dados financeiros entre Administrador e Colaborador,
    /// extração de demandas reais da comunidade e criptografia PBKDF2 de senhas.
    /// </summary>
    public class DashboardAndRegrasNegocioTests
    {
        [Fact(DisplayName = "1. Regra de Negócio RBAC: Colaborador NÃO deve receber saldos ou transações financeiras no Dashboard")]
        public async Task ObterDashboard_PerfilColaborador_DeveOcultarDadosFinanceiros()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.Add(new Carteira { Id = 1, Nome = "Fundo Reserva", Saldo = 25000.00m });
            context.Categoria.Add(new Categoria { Id = 1, Nome = "Doação" });
            context.Transacao.Add(new Transacao { Id = 1, Usuario = 1, Categoria = 1, Razao = "Entrada", Hora = DateTime.Now });
            context.TransacaoFinanceira.Add(new TransacaoFinanceira { Id = 1, Carteira = 1, Valor = 5000.00m });
            context.Voluntario.Add(new Voluntario { Nome = "Voluntário 1", Cpf = "111", Telefone = "11999", Endereco = "Rua 1", Email = "v1@ong.org" });
            await context.SaveChangesAsync();

            var controller = new DashboardApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            // Act
            var resultado = await controller.ObterDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<DashboardDto>(okResult.Value);

            Assert.Equal(1, dto.VoluntariosCadastrados);
            Assert.Equal(0m, dto.SaldoTotal); // Ocultado para Colaborador!
            Assert.Equal(0m, dto.TotalDoacoes); // Ocultado para Colaborador!
            Assert.Empty(dto.UltimasTransacoes); // Lista vazia para Colaborador!
        }

        [Fact(DisplayName = "2. Regra de Negócio RBAC: Administrador DEVE visualizar saldos e transações financeiras completas no Dashboard")]
        public async Task ObterDashboard_PerfilAdministrador_DeveExibirDadosFinanceirosCompletos()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.Add(new Carteira { Id = 1, Nome = "Fundo Reserva", Saldo = 25000.00m });
            context.Categoria.Add(new Categoria { Id = 1, Nome = "Doação" });
            context.Transacao.Add(new Transacao { Id = 1, Usuario = 1, Categoria = 1, Razao = "Entrada", Hora = DateTime.Now });
            context.TransacaoFinanceira.Add(new TransacaoFinanceira { Id = 1, Carteira = 1, Valor = 5000.00m });
            await context.SaveChangesAsync();

            var controller = new DashboardApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            // Act
            var resultado = await controller.ObterDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<DashboardDto>(okResult.Value);

            Assert.Equal(25000.00m, dto.SaldoTotal);
            Assert.Equal(5000.00m, dto.TotalDoacoes);
            Assert.Single(dto.UltimasTransacoes);
        }

        [Fact(DisplayName = "3. Dashboard sem beneficiários cadastrados deve retornar lista de demandas vazia (sem dados fictícios)")]
        public async Task ObterDashboard_SemBeneficiarios_NaoDeveGerarDemandasFicticias()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new DashboardApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            // Act
            var resultado = await controller.ObterDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var dto = Assert.IsType<DashboardDto>(okResult.Value);

            Assert.Equal(0, dto.BeneficiariosCadastrados);
            Assert.Empty(dto.Necessidades);
        }

        [Fact(DisplayName = "4. Segurança: Criptografia PBKDF2 (PasswordHasher) deve validar senha correta e rejeitar incorreta")]
        public void PasswordHasher_DeveGerarHashSeguroEValidarCredenciais()
        {
            // Arrange
            var hasher = new PasswordHasher<Usuario>();
            var usuario = new Usuario { Id = 1, Nome = "Admin Teste", Email = "admin@teste.org" };
            var senhaPura = "Ong@VestindoSonhos2026";

            // Act
            var hashGerado = hasher.HashPassword(usuario, senhaPura);
            var validacaoCorreta = hasher.VerifyHashedPassword(usuario, hashGerado, senhaPura);
            var validacaoIncorreta = hasher.VerifyHashedPassword(usuario, hashGerado, "SenhaErrada123");

            // Assert
            Assert.NotEqual(senhaPura, hashGerado);
            Assert.Equal(PasswordVerificationResult.Success, validacaoCorreta);
            Assert.Equal(PasswordVerificationResult.Failed, validacaoIncorreta);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Models;
using VestindoSonhos.Tests.Helpers;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados de Unidade e Integração do Módulo de Voluntários (API REST).
    /// Valida cadastro por Colaboradores/Administradores, higienização de CPF/Telefone, edição e exclusão.
    /// </summary>
    public class VoluntariosApiTests
    {
        [Fact(DisplayName = "1. Colaborador deve conseguir cadastrar um novo voluntário com higienização de CPF e Telefone")]
        public async Task CriarVoluntario_ComDadosValidos_DeveSalvarEHigienizarDigitos()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new VoluntariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            var novoVoluntario = new VoluntariosApiController.CriarEditarVoluntarioDto
            {
                Nome = "  Maria Clara Souza  ",
                Cpf = "123.456.789-01",
                Telefone = "(11) 98765-4321",
                Email = "maria.clara@vestindosonhos.org",
                Endereco = "Rua das Flores, 120"
            };

            // Act
            var actionResult = await controller.Criar(novoVoluntario);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dtoRetornado = Assert.IsType<VoluntariosApiController.VoluntarioDto>(okResult.Value);

            Assert.Equal("Maria Clara Souza", dtoRetornado.Nome);
            Assert.Equal("12345678901", dtoRetornado.Cpf); // Verifica remoção de pontos e traço
            Assert.Equal("11987654321", dtoRetornado.Telefone); // Verifica remoção de parênteses e hífen
            Assert.Equal(1, context.Voluntario.Count());
        }

        [Theory(DisplayName = "2. Cadastro de voluntário sem nome deve retornar BadRequest (400)")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CriarVoluntario_SemNome_DeveRetornarBadRequest(string nomeInvalido)
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new VoluntariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            var dto = new VoluntariosApiController.CriarEditarVoluntarioDto
            {
                Nome = nomeInvalido,
                Telefone = "11999998888"
            };

            // Act
            var actionResult = await controller.Criar(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(0, context.Voluntario.Count());
        }

        [Fact(DisplayName = "3. Listagem de voluntários deve retornar todos ordenados alfabeticamente")]
        public async Task ObterTodos_DeveRetornarListaOrdenadaPorNome()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Voluntario.AddRange(
                new Voluntario { Nome = "Carlos Eduardo", Cpf = "11122233344", Telefone = "11911112222", Endereco = "Centro", Email = "carlos@ong.org" },
                new Voluntario { Nome = "Ana Beatriz", Cpf = "55566677788", Telefone = "11933334444", Endereco = "Paulista", Email = "ana@ong.org" }
            );
            await context.SaveChangesAsync();

            var controller = new VoluntariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            // Act
            var actionResult = await controller.ObterTodos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<VoluntariosApiController.VoluntarioDto>>(okResult.Value).ToList();

            Assert.Equal(2, lista.Count);
            Assert.Equal("Ana Beatriz", lista[0].Nome);
            Assert.Equal("Carlos Eduardo", lista[1].Nome);
        }

        [Fact(DisplayName = "4. Edição de voluntário existente deve atualizar os dados no banco")]
        public async Task EditarVoluntario_Existente_DeveAtualizarRegistro()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var vol = new Voluntario { Nome = "João Antigo", Cpf = "12312312399", Telefone = "11900001111", Endereco = "Rua A", Email = "joao@ong.org" };
            context.Voluntario.Add(vol);
            await context.SaveChangesAsync();

            var controller = new VoluntariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            var dtoAtualizado = new VoluntariosApiController.CriarEditarVoluntarioDto
            {
                Nome = "João Pedro Atualizado",
                Cpf = "123.123.123-99",
                Telefone = "(11) 98888-7777",
                Endereco = "Rua Nova, 500",
                Email = "joaopedro@ong.org"
            };

            // Act
            var resultado = await controller.Editar(vol.Id, dtoAtualizado);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            var volBanco = await context.Voluntario.FindAsync(vol.Id);
            Assert.NotNull(volBanco);
            Assert.Equal("João Pedro Atualizado", volBanco!.Nome);
            Assert.Equal("11988887777", volBanco.Telefone);
        }

        [Fact(DisplayName = "5. Exclusão de voluntário deve remover vínculos em eventos e excluir o registro")]
        public async Task ExcluirVoluntario_ComEventosAtribuidos_DeveRemoverEmCascata()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var vol = new Voluntario { Nome = "Lucas Mendes", Cpf = "99988877766", Telefone = "11955556666", Endereco = "Av. Brasil", Email = "lucas@ong.org" };
            context.Voluntario.Add(vol);
            await context.SaveChangesAsync();

            context.VoluntarioAtribuido.Add(new VoluntarioAtribuido { Voluntario = vol.Id, Evento = 1 });
            await context.SaveChangesAsync();

            var controller = new VoluntariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            // Act
            var resultado = await controller.Excluir(vol.Id);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(0, context.Voluntario.Count());
            Assert.Equal(0, context.VoluntarioAtribuido.Count());
        }
    }
}

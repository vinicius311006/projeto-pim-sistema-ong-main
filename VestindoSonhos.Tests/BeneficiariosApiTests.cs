using Microsoft.AspNetCore.Mvc;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;
using VestindoSonhos.Tests.Helpers;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados do Módulo de Beneficiários e Dependentes.
    /// Verifica regras de cadastro social, atualização de situação e integridade referencial.
    /// </summary>
    public class BeneficiariosApiTests
    {
        [Fact(DisplayName = "1. Cadastro de beneficiário válido deve persistir no banco com situação padrão 'Ok'")]
        public async Task CriarBeneficiario_Valido_DeveRetornarCreatedAtAction()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new BeneficiariosApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Colaborador");

            var dto = new CriarEditarBeneficiarioDto
            {
                Nome = "Família Silva",
                Telefone = "(11) 94444-3333",
                Endereco = "Rua Solidariedade, 45",
                Situacao = "",
                Observacao = "#Agasalho #Calçados"
            };

            // Act
            var resultado = await controller.Criar(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var beneficiarioCriado = Assert.IsType<BeneficiarioDto>(createdResult.Value);

            Assert.Equal("Família Silva", beneficiarioCriado.Nome);
            Assert.Equal("11944443333", beneficiarioCriado.Telefone);
            Assert.Equal("Ok", beneficiarioCriado.Situacao);
            Assert.Equal(1, context.Beneficiario.Count());
        }

        [Fact(DisplayName = "2. Cadastro de beneficiário sem nome deve ser rejeitado com BadRequest")]
        public async Task CriarBeneficiario_SemNome_DeveRetornarBadRequest()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new BeneficiariosApiController(context);

            var dto = new CriarEditarBeneficiarioDto
            {
                Nome = "   ",
                Telefone = "11999990000"
            };

            // Act
            var resultado = await controller.Criar(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal(0, context.Beneficiario.Count());
        }

        [Fact(DisplayName = "3. Listagem de beneficiários deve contabilizar corretamente o número de dependentes")]
        public async Task ObterTodos_ComDependentesVinculados_DeveContarDependentesCorretamente()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var ben = new Beneficiario
            {
                Nome = "Josefa Oliveira",
                Telefone = "11977776666",
                Endereco = "Vila Esperança, 10",
                Situacao = "Aguardando"
            };
            context.Beneficiario.Add(ben);
            await context.SaveChangesAsync();

            context.Dependente.AddRange(
                new Dependente { Nome = "Pedro Oliveira", IdResponsavel = ben.Id, Idade = 8, Sexo = "M" },
                new Dependente { Nome = "Sofia Oliveira", IdResponsavel = ben.Id, Idade = 5, Sexo = "F" }
            );
            await context.SaveChangesAsync();

            var controller = new BeneficiariosApiController(context);

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<BeneficiarioDto>>(okResult.Value).ToList();

            Assert.Single(lista);
            Assert.Equal(2, lista[0].TotalDependentes);
        }

        [Fact(DisplayName = "4. Exclusão de beneficiário deve excluir também seus dependentes vinculados")]
        public async Task ExcluirBeneficiario_ComDependentes_DeveRemoverDependentesEmCascata()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var ben = new Beneficiario
            {
                Nome = "Roberto Santos",
                Telefone = "11922223333",
                Endereco = "Rua B, 12",
                Situacao = "Ok"
            };
            context.Beneficiario.Add(ben);
            await context.SaveChangesAsync();

            context.Dependente.Add(new Dependente { Nome = "Filho Santos", IdResponsavel = ben.Id, Idade = 10, Sexo = "M" });
            await context.SaveChangesAsync();

            var controller = new BeneficiariosApiController(context);

            // Act
            var resultado = await controller.Excluir(ben.Id);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(0, context.Beneficiario.Count());
            Assert.Equal(0, context.Dependente.Count());
        }
    }
}

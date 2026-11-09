using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using VestindoSonhos.Controllers;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;
using VestindoSonhos.ViewModels;
using VestindoSonhos.Tests.Helpers;
using Xunit;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados de Campos de Input (Sanitização, Limites de Coluna MySQL, Datas e Valores Numéricos).
    /// </summary>
    public class InputValidationTests
    {
        [Fact(DisplayName = "1. Carteiras, Categorias, TiposAtivo e Cargos com apenas espaços em branco devem ser rejeitados")]
        public async Task CadastrosAuxiliares_ComApenasEspacos_DeveRejeitarNoModelState()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();

            // Carteira
            var carteiraCtrl = new Carteiras(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(carteiraCtrl, "Administrador", 1);
            await carteiraCtrl.Criar(new Carteira { Nome = "     ", Saldo = 100 });
            Assert.False(carteiraCtrl.ModelState.IsValid);

            // Categoria
            var categoriaCtrl = new VestindoSonhos.Controllers.Categoria(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(categoriaCtrl, "Administrador", 1);
            await categoriaCtrl.Criar(new VestindoSonhos.Models.Categoria { Nome = "   \t  " }, null);
            Assert.False(categoriaCtrl.ModelState.IsValid);

            // TiposAtivo
            var tiposAtivoCtrl = new VestindoSonhos.Controllers.TiposAtivo(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(tiposAtivoCtrl, "Administrador", 1);
            await tiposAtivoCtrl.Criar(new VestindoSonhos.Models.TiposAtivo { Categoria = "     " });
            Assert.False(tiposAtivoCtrl.ModelState.IsValid);

            // Cargos
            var cargosCtrl = new Cargos(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(cargosCtrl, "Administrador", 1);
            await cargosCtrl.Create(new Cargo { Nome = "    " });
            Assert.False(cargosCtrl.ModelState.IsValid);
        }

        [Fact(DisplayName = "2. Textos gigantes (>300 caracteres) devem sofrer Trim e Truncate para o limite da coluna MySQL")]
        public async Task CadastrosAuxiliares_ComTextoGigante_DeveTruncarSemEstourarBanco()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            string textoGigante = "   " + new string('X', 350) + "   ";

            var carteiraCtrl = new Carteiras(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(carteiraCtrl, "Administrador", 1);
            var carteira = new Carteira { Nome = textoGigante, Saldo = 0 };
            await carteiraCtrl.Criar(carteira);
            Assert.Equal(60, carteira.Nome.Length);

            var cargosCtrl = new Cargos(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(cargosCtrl, "Administrador", 1);
            var cargo = new Cargo { Nome = textoGigante };
            await cargosCtrl.Create(cargo);
            Assert.Equal(50, cargo.Nome.Length);
        }

        [Fact(DisplayName = "3. FinanceiroApiController deve rejeitar valores acima de DECIMAL(10,2) (> 99.999.999,99) ou <= 0")]
        public async Task FinanceiroApi_ComValorAcimaDoLimiteDecimal_DeveRetornar400BadRequest()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, "Administrador", 1);

            // Valor estourando DECIMAL(10,2)
            var resultadoEstouro = await controller.CriarTransacao(new CriarTransacaoFinanceiraDto
            {
                Valor = 150_000_000.00m,
                Razao = "Entrada",
                Observacao = "Teste overflow"
            });
            Assert.IsType<BadRequestObjectResult>(resultadoEstouro);

            // Valor negativo
            var resultadoNegativo = await controller.CriarTransacao(new CriarTransacaoFinanceiraDto
            {
                Valor = -50.00m,
                Razao = "Entrada"
            });
            Assert.IsType<BadRequestObjectResult>(resultadoNegativo);
        }

        [Fact(DisplayName = "4. Eventos (MVC e API) devem rejeitar data de término anterior à data de início")]
        public async Task Eventos_ComDataFimMenorQueDataInicio_DeveRejeitar()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();

            // Teste na API
            var apiCtrl = new EventosApiController(context, NullLogger<EventosApiController>.Instance);
            DbContextTestHelper.ConfigurarUsuarioNoController(apiCtrl, "Administrador", 1);

            var respApi = await apiCtrl.Criar(new CriarEditarEventoDto
            {
                Nome = "Campanha Inverno",
                DataInicio = new DateTime(2026, 11, 20, 14, 0, 0),
                DataFim = new DateTime(2026, 11, 10, 10, 0, 0) // Data Fim antes do Início!
            });
            Assert.IsType<BadRequestObjectResult>(respApi.Result);

            // Teste no MVC
            var mvcCtrl = new Eventos(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(mvcCtrl, "Administrador", 1);

            var vm = new CriacaoEventoViewModel
            {
                evento = new Evento
                {
                    Nome = "Campanha Natal",
                    data_inicio = new DateTime(2026, 12, 25),
                    data_fim = new DateTime(2026, 12, 20)
                }
            };
            await mvcCtrl.Criar(vm);
            Assert.False(mvcCtrl.ModelState.IsValid);
            Assert.True(mvcCtrl.ModelState.ContainsKey("evento.data_fim"));
        }

        [Fact(DisplayName = "5. Voluntários com CPF contendo apenas letras ou menos de 11 dígitos devem ser barrados")]
        public async Task Voluntarios_ComCpfInvalidoOuApenasLetras_DeveRejeitarNoModelState()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new Voluntarios(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, "Administrador", 1);

            var voluntario = new Voluntario
            {
                Nome = "João Teste",
                Cpf = "abc.def.ghi-jk", // Apenas letras -> vira "" após regex
                Telefone = "(16) 99999-8888",
                Email = "joao@teste.com",
                Endereco = "Rua A, 10"
            };

            await controller.Criar(voluntario);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(voluntario.Cpf)));
        }

        [Fact(DisplayName = "6. Inventario Editar sem nenhum produto selecionado deve impedir exclusão órfã dos itens")]
        public async Task InventarioEditar_SemProdutosSelecionados_DeveRetornarErroNoModelState()
        {
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new VestindoSonhos.Controllers.Inventario(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, "Administrador", 1);

            var vm = new TransacaoInventarioCriacaoEdicaoViewModel
            {
                IdTransacao = 10,
                CategoriaTransacao = 1,
                Razao = "Entrada",
                Observacao = "Teste edição vazia",
                Produtos = new List<TransacaoInventarioCriacaoEdicaoViewModel.ItemTransacaoCriacaoEdicao>
                {
                    new() { ProdutoId = 1, Selecionado = false, Quantidade = 0 }
                }
            };

            await controller.Editar(10, vm);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}

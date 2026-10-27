using Microsoft.AspNetCore.Mvc;
using VestindoSonhos.Controllers.Api;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;
using VestindoSonhos.Tests.Helpers;

namespace VestindoSonhos.Tests
{
    /// <summary>
    /// Testes Automatizados do Módulo Financeiro & Carteiras (Exclusivo Administrador).
    /// Garante a exatidão contábil de entradas, saídas, validações de valores e estornos de saldo.
    /// </summary>
    public class FinanceiroApiTests
    {
        [Fact(DisplayName = "1. Lançamento de Entrada (Doação) deve somar corretamente ao saldo da carteira")]
        public async Task CriarTransacao_Entrada_DeveAumentarSaldoDaCarteira()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.Add(new Carteira { Id = 1, Nome = "Caixa Principal", Saldo = 1000.00m });
            context.Categoria.Add(new Categoria { Id = 1, Nome = "Doação" });
            await context.SaveChangesAsync();

            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            var dto = new CriarTransacaoFinanceiraDto
            {
                CarteiraId = 1,
                CategoriaId = 1,
                Razao = "Entrada",
                Valor = 250.50m,
                Observacao = "Doação PIX Campanha de Inverno"
            };

            // Act
            var resultado = await controller.CriarTransacao(dto);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraAtualizada = await context.Carteira.FindAsync(1);
            Assert.Equal(1250.50m, carteiraAtualizada!.Saldo);
            Assert.Equal(1, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "2. Lançamento de Saída (Despesa) deve subtrair corretamente do saldo da carteira")]
        public async Task CriarTransacao_Saida_DeveDiminuirSaldoDaCarteira()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.Add(new Carteira { Id = 1, Nome = "Caixa Principal", Saldo = 1000.00m });
            context.Categoria.Add(new Categoria { Id = 2, Nome = "Logística" });
            await context.SaveChangesAsync();

            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            var dto = new CriarTransacaoFinanceiraDto
            {
                CarteiraId = 1,
                CategoriaId = 2,
                Razao = "Saída",
                Valor = 300.00m,
                Observacao = "Transporte de doações"
            };

            // Act
            var resultado = await controller.CriarTransacao(dto);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraAtualizada = await context.Carteira.FindAsync(1);
            Assert.Equal(700.00m, carteiraAtualizada!.Saldo);
        }

        [Theory(DisplayName = "3. Lançamento financeiro com valor zero ou negativo deve retornar BadRequest")]
        [InlineData(0)]
        [InlineData(-50.00)]
        public async Task CriarTransacao_ValorInvalido_DeveRetornarBadRequest(decimal valorInvalido)
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            var dto = new CriarTransacaoFinanceiraDto
            {
                CarteiraId = 1,
                Razao = "Entrada",
                Valor = valorInvalido
            };

            // Act
            var resultado = await controller.CriarTransacao(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact(DisplayName = "4. Exclusão de transação de Entrada deve estornar (subtrair) o valor do saldo da carteira")]
        public async Task ExcluirTransacao_Entrada_DeveEstornarSaldoDaCarteira()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var carteira = new Carteira { Id = 1, Nome = "Conta Corrente ONG", Saldo = 1500.00m };
            var transacao = new Transacao { Id = 10, Usuario = 1, Categoria = 1, Razao = "Entrada", Hora = DateTime.Now };
            var transacaoFin = new TransacaoFinanceira { Id = 10, Carteira = 1, Valor = 500.00m };

            context.Carteira.Add(carteira);
            context.Transacao.Add(transacao);
            context.TransacaoFinanceira.Add(transacaoFin);
            await context.SaveChangesAsync();

            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            // Act
            var resultado = await controller.ExcluirTransacao(10);

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraBanco = await context.Carteira.FindAsync(1);
            Assert.Equal(1000.00m, carteiraBanco!.Saldo); // 1500 - 500 estornado = 1000
            Assert.Equal(0, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "5. Exclusão de transação no Portal Web (FinanceiroController) também deve estornar o saldo da carteira")]
        public async Task ConfirmarExclusaoWeb_DeveEstornarSaldoDaCarteira()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var carteira = new Carteira { Id = 1, Nome = "Caixa Principal Web", Saldo = 800.00m };
            var transacao = new Transacao { Id = 20, Usuario = 1, Categoria = 1, Razao = "Saída", Hora = DateTime.Now };
            var transacaoFin = new TransacaoFinanceira { Id = 20, Carteira = 1, Valor = 200.00m };

            context.Carteira.Add(carteira);
            context.Transacao.Add(transacao);
            context.TransacaoFinanceira.Add(transacaoFin);
            await context.SaveChangesAsync();

            var webController = new VestindoSonhos.Controllers.FinanceiroController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(webController, cargo: "Administrador");

            // Act
            var resultado = await webController.ConfirmarExclusão(20);

            // Assert
            Assert.IsType<RedirectToActionResult>(resultado);
            var carteiraBanco = await context.Carteira.FindAsync(1);
            Assert.Equal(1000.00m, carteiraBanco!.Saldo); // 800 + 200 (Saída estornada) = 1000
            Assert.Equal(0, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "6. Alteração de categoria de produto no Inventário via API Mobile deve atualizar vínculo corretamente")]
        public async Task EditarProduto_AlterarCategoria_DeveAtualizarCategoriaNoBanco()
        {
            // Arrange
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.TiposAtivo.AddRange(
                new TiposAtivo { Id = 1, Categoria = "Roupas" },
                new TiposAtivo { Id = 2, Categoria = "Calçados" }
            );
            var prod = new Produto { Id = 5, Nome = "Tênis Esportivo", Cor = "Preto", Descricao = "Tam 40", Categoria = 1 };
            context.Produto.Add(prod);
            await context.SaveChangesAsync();

            var invController = new InventarioApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(invController, cargo: "Colaborador");

            // Act
            var resultado = await invController.EditarProduto(5, new CriarProdutoDto
            {
                Nome = "Tênis Esportivo",
                Cor = "Preto",
                Descricao = "Tam 40",
                CategoriaId = 2
            });

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
            var prodAtualizado = await context.Produto.FindAsync(5);
            Assert.Equal(2, prodAtualizado!.Categoria);
        }
    }
}

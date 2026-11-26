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
            // Preparação
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

            // Ação
            var resultado = await controller.CriarTransacao(dto);

            // Verificação
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraAtualizada = await context.Carteira.FindAsync(1);
            Assert.Equal(1250.50m, carteiraAtualizada!.Saldo);
            Assert.Equal(1, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "2. Lançamento de Saída (Despesa) deve subtrair corretamente do saldo da carteira")]
        public async Task CriarTransacao_Saida_DeveDiminuirSaldoDaCarteira()
        {
            // Preparação
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

            // Ação
            var resultado = await controller.CriarTransacao(dto);

            // Verificação
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraAtualizada = await context.Carteira.FindAsync(1);
            Assert.Equal(700.00m, carteiraAtualizada!.Saldo);
        }

        [Theory(DisplayName = "3. Lançamento financeiro com valor zero ou negativo deve retornar BadRequest")]
        [InlineData(0)]
        [InlineData(-50.00)]
        public async Task CriarTransacao_ValorInvalido_DeveRetornarBadRequest(decimal valorInvalido)
        {
            // Preparação
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            var controller = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(controller, cargo: "Administrador");

            var dto = new CriarTransacaoFinanceiraDto
            {
                CarteiraId = 1,
                Razao = "Entrada",
                Valor = valorInvalido
            };

            // Ação
            var resultado = await controller.CriarTransacao(dto);

            // Verificação
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact(DisplayName = "4. Exclusão de transação de Entrada deve estornar (subtrair) o valor do saldo da carteira")]
        public async Task ExcluirTransacao_Entrada_DeveEstornarSaldoDaCarteira()
        {
            // Preparação
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

            // Ação
            var resultado = await controller.ExcluirTransacao(10);

            // Verificação
            Assert.IsType<OkObjectResult>(resultado);
            var carteiraBanco = await context.Carteira.FindAsync(1);
            Assert.Equal(1000.00m, carteiraBanco!.Saldo); // 1500 - 500 estornado = 1000
            Assert.Equal(0, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "5. Exclusão de transação no Portal Web (FinanceiroController) também deve estornar o saldo da carteira")]
        public async Task ConfirmarExclusaoWeb_DeveEstornarSaldoDaCarteira()
        {
            // Preparação
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

            // Ação
            var resultado = await webController.ConfirmarExclusão(20);

            // Verificação
            Assert.IsType<RedirectToActionResult>(resultado);
            var carteiraBanco = await context.Carteira.FindAsync(1);
            Assert.Equal(1000.00m, carteiraBanco!.Saldo); // 800 + 200 (Saída estornada) = 1000
            Assert.Equal(0, context.TransacaoFinanceira.Count());
        }

        [Fact(DisplayName = "6. Alteração de categoria de produto no Inventário via API Mobile deve atualizar vínculo corretamente")]
        public async Task EditarProduto_AlterarCategoria_DeveAtualizarCategoriaNoBanco()
        {
            // Preparação
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

            // Ação
            var resultado = await invController.EditarProduto(5, new CriarProdutoDto
            {
                Nome = "Tênis Esportivo",
                Cor = "Preto",
                Descricao = "Tam 40",
                CategoriaId = 2
            });

            // Verificação
            Assert.IsType<OkObjectResult>(resultado);
            var prodAtualizado = await context.Produto.FindAsync(5);
            Assert.Equal(2, prodAtualizado!.Categoria);
        }

        [Fact(DisplayName = "7. Exclusão da última categoria financeira no Portal Web deve desvincular transações para Sem Categoria sem erro")]
        public async Task ApagarUltimaCategoriaWeb_ComTransacoesVinculadas_DeveDesvincularESucesso()
        {
            // Preparação
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Categoria.Add(new Categoria { Id = 12, Nome = "1" });
            context.Transacao.Add(new Transacao { Id = 30, Usuario = 1, Categoria = 12, Razao = "Entrada", Hora = DateTime.Now });
            await context.SaveChangesAsync();

            var catController = new VestindoSonhos.Controllers.Categoria(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(catController, cargo: "Administrador");

            // Ação
            var resultado = await catController.ApagarConfirmed(12);

            // Verificação
            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal(0, context.Categoria.Count());
            var transacaoBanco = await context.Transacao.FindAsync(30);
            Assert.NotNull(transacaoBanco);
            Assert.Null(transacaoBanco!.Categoria);
        }

        [Fact(DisplayName = "8. Exclusão de todas as categorias financeiras (ApagarTodas Web e ExcluirCategoria API) deve zerar categorias e permitir novos lançamentos")]
        public async Task ExcluirTodasCategorias_WebEApi_DevePermitirZeroCategoriasENovosLancamentos()
        {
            // Preparação
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.Add(new Carteira { Id = 1, Nome = "Caixa Principal", Saldo = 500.00m });
            context.Categoria.AddRange(
                new Categoria { Id = 1, Nome = "Doação" },
                new Categoria { Id = 2, Nome = "Eventos" }
            );
            context.Transacao.AddRange(
                new Transacao { Id = 40, Usuario = 1, Categoria = 1, Razao = "Entrada", Hora = DateTime.Now },
                new Transacao { Id = 41, Usuario = 1, Categoria = 2, Razao = "Saída", Hora = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var catController = new VestindoSonhos.Controllers.Categoria(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(catController, cargo: "Administrador");

            // Ação 1: ApagarTodas no Portal Web
            var resTodas = await catController.ApagarTodas();
            Assert.IsType<RedirectToActionResult>(resTodas);
            Assert.Equal(0, context.Categoria.Count());
            Assert.All(context.Transacao, t => Assert.Null(t.Categoria));

            // Ação 2: Criar nova categoria única e excluí-la via API
            var apiController = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(apiController, cargo: "Administrador");

            context.Categoria.Add(new Categoria { Id = 99, Nome = "Única" });
            context.Transacao.First().Categoria = 99;
            await context.SaveChangesAsync();

            var resDeleteApi = await apiController.ExcluirCategoria(99);
            Assert.IsType<OkObjectResult>(resDeleteApi);
            Assert.Equal(0, context.Categoria.Count());
            Assert.Null(context.Transacao.First().Categoria);

            // Ação 3: Criar transação financeira mesmo com 0 categorias cadastradas
            var resCriarSemCat = await apiController.CriarTransacao(new CriarTransacaoFinanceiraDto
            {
                CarteiraId = 1,
                CategoriaId = null,
                Razao = "Entrada",
                Valor = 100.00m,
                Observacao = "Lançamento sem categoria"
            });
            Assert.IsType<OkObjectResult>(resCriarSemCat);
            Assert.Equal(600.00m, (await context.Carteira.FindAsync(1))!.Saldo);
        }

        [Fact(DisplayName = "9. Edição de todas as carteiras (Portal Web e API) deve permitir alterar nome e saldo livremente")]
        public async Task EditarCarteira_WebEApi_DeveAtualizarSaldoDeTodasAsCarteiras()
        {
            // Preparação com as 3 carteiras padrão
            using var context = DbContextTestHelper.CriarContextoEmMemoria();
            context.Carteira.AddRange(
                new Carteira { Id = 1, Nome = "Conta Principal (Banco)", Saldo = 5000.00m },
                new Carteira { Id = 2, Nome = "Caixa Físico (Sede)", Saldo = 1500.00m },
                new Carteira { Id = 3, Nome = "Fundo de Reserva", Saldo = 10000.00m }
            );
            await context.SaveChangesAsync();

            var webCarteirasController = new VestindoSonhos.Controllers.Carteiras(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(webCarteirasController, cargo: "Administrador");

            // Ação 1: Editar Saldo da Carteira 1 e zerar Saldo da Carteira 3 via Portal Web
            var res1 = await webCarteirasController.Editar(1, new Carteira { Id = 1, Nome = "Conta Principal (Banco)", Saldo = 8250.75m });
            var res3 = await webCarteirasController.Editar(3, new Carteira { Id = 3, Nome = "Fundo de Reserva", Saldo = 0.00m });

            Assert.IsType<RedirectToActionResult>(res1);
            Assert.IsType<RedirectToActionResult>(res3);
            Assert.Equal(8250.75m, (await context.Carteira.FindAsync(1))!.Saldo);
            Assert.Equal(0.00m, (await context.Carteira.FindAsync(3))!.Saldo);

            // Ação 2: Editar Saldo da Carteira 2 via API REST
            var apiController = new FinanceiroApiController(context);
            DbContextTestHelper.ConfigurarUsuarioNoController(apiController, cargo: "Administrador");

            var resApi2 = await apiController.EditarCarteira(2, new CarteiraResumoDto
            {
                Id = 2,
                Nome = "Caixa Físico (Sede)",
                Saldo = 2750.50m
            });

            Assert.IsType<OkObjectResult>(resApi2);
            Assert.Equal(2750.50m, (await context.Carteira.FindAsync(2))!.Saldo);
        }
    }
}

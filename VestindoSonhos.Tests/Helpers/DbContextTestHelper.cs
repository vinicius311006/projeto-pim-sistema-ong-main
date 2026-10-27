using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VestindoSonhos.Models;

namespace VestindoSonhos.Tests.Helpers
{
    /// <summary>
    /// Classe utilitária para criação de bancos de dados em memória (EF Core InMemory)
    /// e simulação de contextos de autenticação (Administrador / Colaborador) nos testes automatizados.
    /// </summary>
    public static class DbContextTestHelper
    {
        public static VestindosonhosdbContext CriarContextoEmMemoria(string? nomeBanco = null)
        {
            var options = new DbContextOptionsBuilder<VestindosonhosdbContext>()
                .UseInMemoryDatabase(databaseName: nomeBanco ?? Guid.NewGuid().ToString())
                .Options;

            var context = new VestindosonhosdbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public static void ConfigurarUsuarioNoController(ControllerBase controller, string cargo = "Administrador", int userId = 1)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Name, "usuario.teste@vestindosonhos.org"),
                new Claim("Nome", $"Usuário {cargo}"),
                new Claim(ClaimTypes.Role, cargo)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}

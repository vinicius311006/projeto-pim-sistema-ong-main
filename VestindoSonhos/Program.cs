using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using VestindoSonhos.Helpers;
using VestindoSonhos.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Configuração do DbContext (lê de User Secrets ou Variáveis de Ambiente)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("ConnectionStrings");
}
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
}
if (string.IsNullOrWhiteSpace(connectionString))
{
    var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var dbHost = inContainer ? "db" : "localhost";
    connectionString = $"Server={dbHost};Port=3306;Database=VESTINDOSONHOSDB;Uid=root;Pwd=root;CharSet=utf8mb4;";
}

builder.Services.AddDbContext<VestindosonhosdbContext>(options =>
{
    var connLower = (connectionString ?? string.Empty).ToLowerInvariant();
    if (!string.IsNullOrEmpty(connectionString) && (connLower.Contains("3306") || connLower.Contains("user=") || connLower.Contains("uid=") || connLower.Contains("server=db") || connLower.Contains("vestindosonhosdb") || connLower.Contains("mysql")))
    {
        options.UseMySQL(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Configurações do JWT (lê de User Secrets, Variáveis de Ambiente ou gera chave criptográfica efêmera segura)
var jwtKey = JwtKeyHelper.ObterChaveJwt(builder.Configuration);
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VestindoSonhosAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VestindoSonhosMobile";

// Proteção contra Força Bruta e Negação de Serviço (Rate Limiting)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Limite global por IP: até 180 requisições por minuto
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 180,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Limite estrito para rotas de Login: até 10 tentativas por minuto por IP
    options.AddPolicy("login-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "login",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(
                "{\"mensagem\":\"Muitas tentativas realizadas em curto período. Aguarde 1 minuto antes de tentar novamente.\"}",
                token);
        }
        else
        {
            context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            await context.HttpContext.Response.WriteAsync(
                "Muitas tentativas realizadas em curto período. Por segurança, aguarde 1 minuto e tente novamente.",
                token);
        }
    };
});

// Configuração de Autenticação Híbrida (Cookie para Web MVC + JWT Bearer para API REST)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddPolicyScheme("SmartScheme", "Cookie or JWT Bearer", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "VestindoSonhos.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Erro/AcessoNegado";
    options.LoginPath = "/Login";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração Global de Cultura e Localização PT-BR
var culturaPtBr = new System.Globalization.CultureInfo("pt-BR");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culturaPtBr;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culturaPtBr;

// Adiciona os serviços ao contêiner de injeção de dependência.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add<PtBrModelStateFilter>();
    options.ModelMetadataDetailsProviders.Add(new PtBrMetadataProvider());

    // Tradução global de todas as mensagens internas de Model Binding do ASP.NET Core para PT-BR
    var mb = options.ModelBindingMessageProvider;
    mb.SetValueMustNotBeNullAccessor(x => $"O campo '{PtBrMetadataProvider.ObterNomeCampoPtBr(x)}' é obrigatório.");
    mb.SetAttemptedValueIsInvalidAccessor((x, y) => $"O valor '{x}' não é válido para o campo '{PtBrMetadataProvider.ObterNomeCampoPtBr(y)}'.");
    mb.SetMissingBindRequiredValueAccessor(x => $"O valor para o campo '{PtBrMetadataProvider.ObterNomeCampoPtBr(x)}' não foi fornecido.");
    mb.SetMissingKeyOrValueAccessor(() => "Este campo é obrigatório.");
    mb.SetUnknownValueIsInvalidAccessor(x => $"O valor informado é inválido para o campo '{PtBrMetadataProvider.ObterNomeCampoPtBr(x)}'.");
    mb.SetValueIsInvalidAccessor(x => $"O valor '{x}' é inválido.");
    mb.SetValueMustBeANumberAccessor(x => $"O campo '{PtBrMetadataProvider.ObterNomeCampoPtBr(x)}' deve ser um número válido.");
    mb.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição não pode estar vazio.");
    mb.SetNonPropertyAttemptedValueIsInvalidAccessor(x => $"O valor '{x}' não é válido.");
    mb.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado é inválido.");
    mb.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número válido.");
});
builder.Services.AddRazorPages();

var app = builder.Build();

var opcoesLocalizacao = new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR")
    .AddSupportedCultures("pt-BR")
    .AddSupportedUICultures("pt-BR");
app.UseRequestLocalization(opcoesLocalizacao);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VestindosonhosdbContext>();
    try { db.Database.ExecuteSqlRaw("CREATE OR REPLACE VIEW Cargo AS SELECT * FROM Cargos;"); } catch { }
    try { db.Database.ExecuteSqlRaw("CREATE OR REPLACE VIEW Evento AS SELECT * FROM Eventos;"); } catch { }
    try { db.Database.ExecuteSqlRaw("CREATE OR REPLACE VIEW Tipos_ativo AS SELECT * FROM TiposAtivo;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente MODIFY COLUMN ano_nascimento INT NULL DEFAULT 0;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente ADD COLUMN idade INT NOT NULL DEFAULT 0;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente ADD COLUMN observacao VARCHAR(255) NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Produto MODIFY COLUMN descricao VARCHAR(255) NULL DEFAULT '';"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Produto MODIFY COLUMN cor VARCHAR(15) NULL DEFAULT '';"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Categoria MODIFY COLUMN nome VARCHAR(60) NOT NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE TiposAtivo MODIFY COLUMN categoria VARCHAR(60) NOT NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Carteira MODIFY COLUMN nome VARCHAR(60) NOT NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Transacao DROP FOREIGN KEY FK_Transacao_Categoria;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Transacao MODIFY COLUMN categoria INT NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE Transacao SET categoria = NULL WHERE categoria IS NOT NULL AND categoria NOT IN (SELECT id FROM Categoria);"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Transacao ADD CONSTRAINT FK_Transacao_Categoria FOREIGN KEY (categoria) REFERENCES Categoria(id) ON DELETE SET NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE TiposAtivo SET categoria = 'Calçados' WHERE id = 2 OR categoria LIKE 'Cal%ados';"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE Categoria SET nome = 'Doação' WHERE nome LIKE 'Doa%o';"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE Carteira SET nome = 'Caixa Físico (Sede)' WHERE nome LIKE 'Caixa F%sico%';"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE Eventos SET local = 'Ginásio Municipal', observacao = 'Arrecadação de casacos e cobertores' WHERE id = 1;"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE Eventos SET observacao = 'Venda comunitária e distribuição' WHERE id = 2;"); } catch { }

    // Auto-reparo caso alguma transação tenha sido inflada pelo bug de parsing (ex: 1500 virando 150000)
    try
    {
        var transacoesInfladas = db.TransacaoFinanceira.Where(t => t.Valor >= 100000m).ToList();
        foreach (var ti in transacoesInfladas)
        {
            if (ti.Valor % 100 == 0)
            {
                decimal valorCorreto = ti.Valor / 100m;
                decimal diferenca = ti.Valor - valorCorreto;
                ti.Valor = valorCorreto;

                var cart = db.Carteira.FirstOrDefault(c => c.Id == ti.Carteira);
                if (cart != null)
                {
                    var trans = db.Transacao.FirstOrDefault(tr => tr.Id == ti.Id);
                    if (trans?.Razao == "Entrada" || trans?.Razao == null)
                    {
                        cart.Saldo -= diferenca;
                    }
                    else
                    {
                        cart.Saldo += diferenca;
                    }
                }
            }
        }
        db.SaveChanges();
    }
    catch { }
}

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.SameAsRequest
};

app.UseCookiePolicy(cookiePolicyOptions);

// Configura o pipeline de requisições HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // O valor padrão do HSTS é de 30 dias. Pode ser ajustado para cenários de produção (consulte https://aka.ms/aspnetcore-hsts).
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Interceptador para forçar alteração de senha temporária (Web e API)
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && context.User.HasClaim(c => c.Type == "ExigeTrocaSenha" && c.Value == "true"))
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (path.StartsWith("/api"))
        {
            if (!path.StartsWith("/api/auth/trocar-senha-obrigatoria") &&
                !path.StartsWith("/api/auth/login"))
            {
                var userIdStr = context.User.FindFirst("UserId")?.Value
                    ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdStr, out int userId))
                {
                    var db = context.RequestServices.GetRequiredService<VestindosonhosdbContext>();
                    var usuario = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (usuario?.ExigirTrocaSenha == true)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"mensagem\":\"Troca de senha temporária obrigatória antes de acessar os recursos do sistema.\"}");
                        return;
                    }
                }
            }
        }
        else if (!path.StartsWith("/login/trocarsenhaobrigatoria") &&
                 !path.StartsWith("/login/logout") &&
                 !path.StartsWith("/css") &&
                 !path.StartsWith("/js") &&
                 !path.StartsWith("/lib") &&
                 !path.StartsWith("/img") &&
                 !path.StartsWith("/images") &&
                 !path.StartsWith("/mobile"))
        {
            context.Response.Redirect("/Login/TrocarSenhaObrigatoria");
            return;
        }
    }
    await next();
});

app.MapControllers();
app.MapDefaultControllerRoute();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

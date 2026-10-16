using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VestindoSonhos.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000;http://localhost:5000");

// Configuração do DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("ConnectionStrings");

builder.Services.AddDbContext<VestindosonhosdbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString) && (connectionString.Contains("Port=3306") || connectionString.Contains("User=") || connectionString.Contains("uid=")))
    {
        options.UseMySQL(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
    options.EnableSensitiveDataLogging();
});

// Configurações do JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "VestindoSonhos_ChaveSecretaSuperSegura_JWT_2026_Key!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VestindoSonhosAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VestindoSonhosMobile";

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
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VestindosonhosdbContext>();
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente MODIFY COLUMN ano_nascimento INT NULL DEFAULT 0;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente ADD COLUMN idade INT NOT NULL DEFAULT 0;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Dependente ADD COLUMN observacao VARCHAR(255) NULL;"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Produto MODIFY COLUMN descricao VARCHAR(255) NULL DEFAULT '';"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Produto MODIFY COLUMN cor VARCHAR(15) NULL DEFAULT '';"); } catch { }
}

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
};

app.UseCookiePolicy(cookiePolicyOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Interceptador para forçar alteração de senha temporária
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && context.User.HasClaim(c => c.Type == "ExigeTrocaSenha" && c.Value == "true"))
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (!path.StartsWith("/login/trocarsenhaobrigatoria") &&
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

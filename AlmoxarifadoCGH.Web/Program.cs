using Almoxarifado.Infrastructure.Data;
using AlmoxarifadoCGH.Web.Components;
using AlmoxarifadoCGH.Web.Security;
using AlmoxarifadoCGH.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;


var builder = WebApplication.CreateBuilder(args);
// 1. Mude para AddDbContextFactory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Mantenha o serviço como Scoped
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddMudServices();

// Ativa os núcleos de segurança do Blazor
//builder.Services.AddAuthenticationCore();
//builder.Services.AddAuthorizationCore(); // <-- Esta é a linha que resolve o loop!
//builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ============================================================
// MAGIA PLUG AND PLAY: Cria a base de dados automaticamente
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var context = factory.CreateDbContext();
    context.Database.Migrate(); // Se o ficheiro GuardaDigital.db não existir, ele cria na hora!
}
// ============================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

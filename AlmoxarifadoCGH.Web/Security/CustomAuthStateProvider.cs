using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace AlmoxarifadoCGH.Web.Security;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;

    public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // DESATIVAÇÃO TEMPORÁRIA: Força um usuário fixo para bypass do login
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Operador_Temporario")
        }, "CustomAuth"));

        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public async Task AtualizarEstadoAutenticacao(string? nomeUsuario)
    {
        // Método mantido vazio temporariamente para evitar erros de compilação
        await Task.CompletedTask;
    }
}
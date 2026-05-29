using Almoxarifado.Domain.Entities;
using Almoxarifado.Domain.Enums;
using Almoxarifado.Infrastructure.Data;
using AlmoxarifadoCGH.Web.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Almoxarifado.Tests;

// 1. FÁBRICA FALSA: Garante que os testes usam uma base de dados virtual rápida e isolada
public class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly string _databaseName;

    public TestDbContextFactory(string databaseName)
    {
        _databaseName = databaseName;
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AppDbContext(options);
    }
}

// 2. OS TESTES
public class EstoqueServiceTests
{
    [Fact]
    public async Task EntregarMochila_ComOMesmoAgente_DeveLancarExcecao()
    {
        // Arrange (Preparação)
        // Criamos uma base de dados virtual específica para este teste
        var factory = new TestDbContextFactory("DbTeste_MesmoAgente");
        var service = new EstoqueService(factory);

        int mochilaId = 1;
        int agenteId = 10; // O ID de teste do agente

        // Act (Ação) & Assert (Verificação)
        // Tentamos entregar a mochila passando o agente 10 para as duas posições da dupla
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EntregarMochilaAsync(mochilaId, "Manhã", agenteId, agenteId));

        // Verificamos se a mensagem de erro que o sistema devolveu é exatamente a que programámos
        Assert.Equal("Selecione dois agentes diferentes. O mesmo agente não pode fazer a dupla sozinho.", excecao.Message);
    }

    [Fact]
    public async Task EntregarMochila_SemAgentesSelecionados_DeveLancarExcecao()
    {
        // Arrange (Preparação)
        var factory = new TestDbContextFactory("DbTeste_SemAgente");
        var service = new EstoqueService(factory);

        // Act (Ação) & Assert (Verificação)
        // Tentamos entregar deixando o Agente 1 vazio (ID 0 que vem da dropdown)
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EntregarMochilaAsync(1, "Manhã", 0, 5));

        Assert.Equal("Você deve selecionar dois agentes para formar a dupla.", excecao.Message);
    }

    [Fact]
    public async Task AdicionarMochila_ComNumeroJaExistente_DeveLancarExcecao()
    {
        // Arrange (Preparação)
        var factory = new TestDbContextFactory("DbTeste_MochilaDuplicada");
        var service = new EstoqueService(factory);

        // Inserimos a Mochila 1 na base de dados virtual
        await service.AdicionarMochilaAsync(1);

        // Act & Assert
        // Tentamos criar outra Mochila com o número 1
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdicionarMochilaAsync(1));

        Assert.Equal("A Mochila número 1 já está cadastrada.", excecao.Message);
    }
}
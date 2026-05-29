using Almoxarifado.Domain.Entities;
using Almoxarifado.Domain.Enums;
using Almoxarifado.Domain.ValueObjects;

namespace Almoxarifado.Tests;

public class MochilaTests
{
    [Fact]
    public void CriarMochila_DeveInicializarComoDisponivel()
    {
        // Arrange & Act
        var mochila = new Mochila(1);

        // Assert
        Assert.Equal(1, mochila.Numero);
        Assert.Equal(StatusMochila.Disponivel, mochila.Status);
    }

    [Fact]
    public void EntregarParaDupla_DeveMudarStatusParaEmUso()
    {
        // Arrange
        var mochila = new Mochila(2);

        // Simula 4 equipamentos disponíveis para enganar a regra de negócio e montar o kit
        var equipamentosFalsos = new List<Equipamento>
        {
            new Equipamento { Id = 1, NumeroSerie = "EQ01", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 2, NumeroSerie = "EQ02", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 3, NumeroSerie = "EQ03", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 4, NumeroSerie = "EQ04", Status = StatusEquipamento.Disponivel }
        };
        mochila.MontarKit(equipamentosFalsos);

        var operacao = new OperacaoTurno("Manhã", "João e Maria");

        // Act
        mochila.EntregarParaDupla(operacao);

        // Assert
        Assert.Equal(StatusMochila.EmUso, mochila.Status);
        Assert.Equal(operacao, mochila.OperacaoAtual);
    }

    [Fact]
    public void DevolverMochila_DeveLimparOperacaoERetornarParaDisponivel()
    {
        // Arrange
        var mochila = new Mochila(3);

        // Novamente, prepara a mochila com os 4 equipamentos para deixá-la válida
        var equipamentosFalsos = new List<Equipamento>
        {
            new Equipamento { Id = 1, NumeroSerie = "EQ01", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 2, NumeroSerie = "EQ02", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 3, NumeroSerie = "EQ03", Status = StatusEquipamento.Disponivel },
            new Equipamento { Id = 4, NumeroSerie = "EQ04", Status = StatusEquipamento.Disponivel }
        };
        mochila.MontarKit(equipamentosFalsos);

        mochila.EntregarParaDupla(new OperacaoTurno("Tarde", "Carlos e Ana"));

        // Act
        mochila.Devolver();

        // Assert
        Assert.Equal(StatusMochila.Disponivel, mochila.Status);
        Assert.Null(mochila.OperacaoAtual);
    }
}
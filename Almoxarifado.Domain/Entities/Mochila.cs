using Almoxarifado.Domain.Enums;
using Almoxarifado.Domain.ValueObjects;

namespace Almoxarifado.Domain.Entities;

public class Mochila // Aggregate Root 
{
    public int Id { get; private set; }
    public int Numero { get; private set; } // 1 a 12
    public StatusMochila Status { get; private set; }

    // Opcional: Saber com quem está no momento
    public OperacaoTurno? OperacaoAtual { get; private set; }

    // Encapsulamento da lista: Ninguém de fora pode dar um ".Add()" direto aqui.
    private readonly List<Equipamento> _equipamentos = new();
    public IReadOnlyCollection<Equipamento> Equipamentos => _equipamentos.AsReadOnly();

    // Construtor para o Entity Framework e criação inicial
    protected Mochila() { }

    public Mochila(int numero)
    {
        if (numero < 1 || numero > 12)
            throw new ArgumentException("A mochila deve ser numerada de 1 a 12.");

        Numero = numero;
        Status = StatusMochila.Disponivel;
    }

    // --- COMPORTAMENTOS DO DOMÍNIO (Atividades do Storytelling) ---

    public void MontarKit(IEnumerable<Equipamento> equipamentos)
    {
        if (Status != StatusMochila.Disponivel)
            throw new InvalidOperationException("Só é possível montar kits em mochilas disponíveis.");

        // Aqui a mochila garante suas próprias regras!
        if (equipamentos.Count() != 4)
            throw new ArgumentException("O kit deve conter exatamente 4 equipamentos (1 Tablet, 1 DWS, 2 Rádios).");

        _equipamentos.Clear();
        _equipamentos.AddRange(equipamentos);
    }

    public void EntregarParaDupla(OperacaoTurno operacao)
    {
        if (Status != StatusMochila.Disponivel)
            throw new InvalidOperationException($"A mochila {Numero} não está disponível para entrega.");

        if (_equipamentos.Count != 4)
            throw new InvalidOperationException("Não é possível entregar uma mochila com o kit incompleto.");

        OperacaoAtual = operacao;
        Status = StatusMochila.EmUso;

        // Atualiza o status de todos os itens internos
        foreach (var equipamento in _equipamentos)
        {
            equipamento.Status = StatusEquipamento.Emprestado;
        }
    }

    public void Devolver()
    {
        if (Status != StatusMochila.EmUso)
            throw new InvalidOperationException($"A mochila {Numero} já consta como devolvida ou disponível.");

        OperacaoAtual = null;
        Status = StatusMochila.Disponivel;

        // Ao devolver, os equipamentos vão para recarga, conforme você explicou no processo real.
        foreach (var equipamento in _equipamentos)
        {
            // Nota: Precisaremos adicionar StatusEquipamento.Carregando no seu enum atual
            equipamento.Status = StatusEquipamento.Manutencao; // Usando manutenção provisoriamente até atualizar o enum
        }
    }

    public void SubstituirEquipamentoQuebrado(string numeroSerieQuebrado, Equipamento novoEquipamento)
    {
        var equipamentoAntigo = _equipamentos.FirstOrDefault(e => e.NumeroSerie == numeroSerieQuebrado);

        if (equipamentoAntigo == null)
            throw new InvalidOperationException("O equipamento informado não pertence a esta mochila.");

        _equipamentos.Remove(equipamentoAntigo);
        _equipamentos.Add(novoEquipamento);

        // O equipamento quebrado vai para manutenção
        equipamentoAntigo.Status = StatusEquipamento.Manutencao;
    }
}
namespace Almoxarifado.Domain.ValueObjects;

// Value Object: Imutável, representa a equipe e o turno que estão com a mochila.
public class OperacaoTurno
{
    public string Turno { get; } // Ex: "T1", "T2", "T3"
    public string Dupla { get; } // Identificação da dupla

    public OperacaoTurno(string turno, string dupla)
    {
        if (string.IsNullOrWhiteSpace(turno)) throw new ArgumentException("Turno é obrigatório.");
        if (string.IsNullOrWhiteSpace(dupla)) throw new ArgumentException("Dupla é obrigatória.");

        Turno = turno;
        Dupla = dupla;
    }
}

public enum StatusMochila
{
    Disponivel = 1,
    EmUso = 2,
    Manutencao = 3 // Caso falte algum item e não possa ser entregue
}
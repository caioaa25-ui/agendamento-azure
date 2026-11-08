namespace Agendamento.Domain.Entities;

public sealed class Consulta
{
    public Guid Id { get; }
    public string Paciente { get; }
    public DateTimeOffset Inicio { get; }
    public TimeSpan Duracao { get; }

    public Consulta(string paciente, DateTimeOffset inicio, TimeSpan duracao)
    {
        if (string.IsNullOrWhiteSpace(paciente))
            throw new ArgumentException("O paciente é obrigatório.", nameof(paciente));

        if (duracao <= TimeSpan.Zero)
            throw new ArgumentException("A duração deve ser maior que zero.", nameof(duracao));

        Id = Guid.NewGuid();
        Paciente = paciente;
        Inicio = inicio;
        Duracao = duracao;
    }

    public DateTimeOffset Fim => Inicio + Duracao;
}

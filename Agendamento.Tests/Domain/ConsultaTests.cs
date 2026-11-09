using Agendamento.Domain.Entities;
using Xunit;

namespace Agendamento.Tests.Domain;

public class ConsultaTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 11, 10, 9, 0, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void Fim_e_inicio_mais_duracao()
    {
        var consulta = new Consulta("Maria", Inicio, TimeSpan.FromMinutes(30));

        Assert.Equal(Inicio.AddMinutes(30), consulta.Fim);
    }

    [Fact]
    public void Rejeita_paciente_vazio()
    {
        Assert.Throws<ArgumentException>(() => new Consulta("  ", Inicio, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Rejeita_duracao_zero()
    {
        Assert.Throws<ArgumentException>(() => new Consulta("Maria", Inicio, TimeSpan.Zero));
    }
}
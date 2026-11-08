using Agendamento.Application.Abstractions;
using Agendamento.Application.UseCases;
using Agendamento.Domain.Entities;
using Xunit;

namespace Agendamento.Tests.UseCases;

public class AgendarConsultaUseCaseTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 11, 10, 9, 0, 0, TimeSpan.FromHours(-3));

    [Fact]
    public async Task Agenda_quando_horario_esta_livre()
    {
        var repositorio = new RepositorioEmMemoria();
        var useCase = new AgendarConsultaUseCase(repositorio);

        var consulta = await useCase.ExecutarAsync("Maria", Inicio, TimeSpan.FromMinutes(30), CancellationToken.None);

        Assert.Equal("Maria", consulta.Paciente);
        Assert.Single(repositorio.Consultas);
    }

    [Fact]
    public async Task Rejeita_horario_sobreposto()
    {
        var repositorio = new RepositorioEmMemoria();
        var useCase = new AgendarConsultaUseCase(repositorio);

        await useCase.ExecutarAsync("Maria", Inicio, TimeSpan.FromMinutes(60), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecutarAsync("Joao", Inicio.AddMinutes(30), TimeSpan.FromMinutes(60), CancellationToken.None));
    }

    [Fact]
    public async Task Permite_consulta_logo_apos_o_fim_de_outra()
    {
        var repositorio = new RepositorioEmMemoria();
        var useCase = new AgendarConsultaUseCase(repositorio);

        await useCase.ExecutarAsync("Maria", Inicio, TimeSpan.FromMinutes(60), CancellationToken.None);
        await useCase.ExecutarAsync("Joao", Inicio.AddMinutes(60), TimeSpan.FromMinutes(60), CancellationToken.None);

        Assert.Equal(2, repositorio.Consultas.Count);
    }

    private sealed class RepositorioEmMemoria : IConsultaRepository
    {
        public List<Consulta> Consultas { get; } = [];

        public Task<IReadOnlyList<Consulta>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Consulta>>(Consultas.Where(c => c.Inicio < fim && inicio < c.Fim).ToList());

        public Task AdicionarAsync(Consulta consulta, CancellationToken cancellationToken)
        {
            Consultas.Add(consulta);
            return Task.CompletedTask;
        }
    }
}

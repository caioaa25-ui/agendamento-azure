using Agendamento.Application.Abstractions;
using Agendamento.Domain.Entities;

namespace Agendamento.Infrastructure.Persistence;

public sealed class ConsultaRepositoryEmMemoria : IConsultaRepository
{
    private readonly List<Consulta> _consultas = [];
    private readonly object _trava = new();

    public Task<IReadOnlyList<Consulta>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim, CancellationToken cancellationToken)
    {
        lock (_trava)
        {
            IReadOnlyList<Consulta> resultado = _consultas.Where(c => c.Inicio < fim && inicio < c.Fim).ToList();
            return Task.FromResult(resultado);
        }
    }

    public Task AdicionarAsync(Consulta consulta, CancellationToken cancellationToken)
    {
        lock (_trava)
        {
            _consultas.Add(consulta);
        }
        return Task.CompletedTask;
    }
}

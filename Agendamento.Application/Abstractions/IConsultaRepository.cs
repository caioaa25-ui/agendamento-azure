using Agendamento.Domain.Entities;

namespace Agendamento.Application.Abstractions;

public interface IConsultaRepository
{
    Task<IReadOnlyList<Consulta>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim, CancellationToken cancellationToken);
    Task AdicionarAsync(Consulta consulta, CancellationToken cancellationToken);
}

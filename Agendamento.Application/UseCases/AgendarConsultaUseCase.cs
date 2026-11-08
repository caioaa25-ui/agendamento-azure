using Agendamento.Application.Abstractions;
using Agendamento.Domain.Entities;

namespace Agendamento.Application.UseCases;

public sealed class AgendarConsultaUseCase(IConsultaRepository repositorio)
{
    public async Task<Consulta> ExecutarAsync(string paciente, DateTimeOffset inicio, TimeSpan duracao, CancellationToken cancellationToken)
    {
        var consulta = new Consulta(paciente, inicio, duracao);

        var existentes = await repositorio.ObterPorPeriodoAsync(consulta.Inicio, consulta.Fim, cancellationToken);

        if (existentes.Any(c => consulta.Inicio < c.Fim && c.Inicio < consulta.Fim))
            throw new InvalidOperationException("Já existe uma consulta nesse horário.");

        await repositorio.AdicionarAsync(consulta, cancellationToken);
        return consulta;
    }
}

using Newtonsoft.Json;
using Agendamento.Application.Abstractions;
using Agendamento.Domain.Entities;
using Microsoft.Azure.Cosmos;

namespace Agendamento.Infrastructure.Persistence;

public sealed class ConsultaRepositoryCosmos(Container container) : IConsultaRepository
{
    public async Task<IReadOnlyList<Consulta>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim, CancellationToken cancellationToken)
    {
        var resultado = new List<Consulta>();
        var iterador = container.GetItemQueryIterator<ConsultaDocumento>("SELECT * FROM c");

        while (iterador.HasMoreResults)
        {
            foreach (var documento in await iterador.ReadNextAsync(cancellationToken))
            {
                var consulta = documento.ParaConsulta();
                if (consulta.Inicio < fim && inicio < consulta.Fim)
                    resultado.Add(consulta);
            }
        }

        return resultado;
    }

    public async Task AdicionarAsync(Consulta consulta, CancellationToken cancellationToken)
    {
        var documento = ConsultaDocumento.De(consulta);
        await container.CreateItemAsync(documento, new PartitionKey(consulta.Paciente), cancellationToken: cancellationToken);
    }

    private sealed class ConsultaDocumento
    {
        [JsonProperty("id")] public string Id { get; set; } = "";
        [JsonProperty("paciente")] public string Paciente { get; set; } = "";
        public DateTimeOffset Inicio { get; set; }
        public double DuracaoMinutos { get; set; }

        public static ConsultaDocumento De(Consulta consulta) => new()
        {
            Id = consulta.Id.ToString(),
            Paciente = consulta.Paciente,
            Inicio = consulta.Inicio,
            DuracaoMinutos = consulta.Duracao.TotalMinutes
        };

        public Consulta ParaConsulta() => new(Paciente, Inicio, TimeSpan.FromMinutes(DuracaoMinutos));
    }
}

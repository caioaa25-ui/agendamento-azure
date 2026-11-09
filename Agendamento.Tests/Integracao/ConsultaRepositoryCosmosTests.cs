using Agendamento.Domain.Entities;
using Agendamento.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Agendamento.Tests.Integracao;

public class ConsultaRepositoryCosmosTests
{
    [Fact]
    public async Task Grava_e_le_consulta_no_cosmos()
    {
        var endpoint = Environment.GetEnvironmentVariable("CosmosEndpoint");
        var chave = Environment.GetEnvironmentVariable("CosmosKey");
        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(chave))
            return;

        var container = new CosmosClient(endpoint, chave).GetContainer("agendamento", "consultas");
        var repositorio = new ConsultaRepositoryCosmos(container);

        var paciente = "Teste-" + Guid.NewGuid();
        var inicio = new DateTimeOffset(2030, 1, 1, 9, 0, 0, TimeSpan.FromHours(-3));
        var consulta = new Consulta(paciente, inicio, TimeSpan.FromMinutes(30));

        try
        {
            await repositorio.AdicionarAsync(consulta, CancellationToken.None);

            var encontradas = await repositorio.ObterPorPeriodoAsync(inicio, inicio.AddHours(1), CancellationToken.None);

            Assert.Contains(encontradas, c => c.Paciente == paciente);
        }
        finally
        {
            await container.DeleteItemAsync<object>(consulta.Id.ToString(), new PartitionKey(paciente));
        }
    }
}
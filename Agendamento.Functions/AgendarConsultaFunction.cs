using System.Net;
using Agendamento.Application.UseCases;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace Agendamento.Functions;

public sealed class AgendarConsultaFunction(AgendarConsultaUseCase useCase)
{
    public sealed record Requisicao(string Paciente, DateTimeOffset Inicio, int DuracaoMinutos);

    [Function("AgendarConsulta")]
    public async Task<HttpResponseData> Executar(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consultas")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var corpo = await JsonSerializer.DeserializeAsync<Requisicao>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
        if (corpo is null)
            return await Resposta(req, HttpStatusCode.BadRequest, new { erro = "Corpo inválido." });

        try
        {
            var consulta = await useCase.ExecutarAsync(corpo.Paciente, corpo.Inicio, TimeSpan.FromMinutes(corpo.DuracaoMinutos), cancellationToken);
            return await Resposta(req, HttpStatusCode.Created, new { consulta.Id, consulta.Paciente, consulta.Inicio, consulta.Fim });
        }
        catch (InvalidOperationException ex)
        {
            return await Resposta(req, HttpStatusCode.Conflict, new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return await Resposta(req, HttpStatusCode.BadRequest, new { erro = ex.Message });
        }
    }

    private static async Task<HttpResponseData> Resposta(HttpRequestData req, HttpStatusCode status, object corpo)
    {
        var resposta = req.CreateResponse(status);
        await resposta.WriteAsJsonAsync(corpo);
        return resposta;
    }
}



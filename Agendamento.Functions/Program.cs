using Microsoft.Azure.Cosmos;
using Agendamento.Application.Abstractions;
using Agendamento.Application.UseCases;
using Agendamento.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddSingleton(new CosmosClient(Environment.GetEnvironmentVariable("CosmosEndpoint"), Environment.GetEnvironmentVariable("CosmosKey")).GetContainer("agendamento", "consultas"));
builder.Services.AddSingleton<IConsultaRepository, ConsultaRepositoryCosmos>();
builder.Services.AddScoped<AgendarConsultaUseCase>();
builder.Build().Run();


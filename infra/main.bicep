@description('Nome da conta do Cosmos DB')
param nomeConta string = 'cosmosagendamento2026'

@description('Regiao dos recursos')
param localizacao string = resourceGroup().location

@description('Nome da Function App (precisa ser unico no Azure)')
param nomeFunction string = 'func-agendamento-caioaa25'

resource conta 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: nomeConta
  location: localizacao
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: localizacao
        failoverPriority: 0
      }
    ]
  }
}

resource banco 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: conta
  name: 'agendamento'
  properties: {
    resource: {
      id: 'agendamento'
    }
    options: {
      throughput: 1000
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: banco
  name: 'consultas'
  properties: {
    resource: {
      id: 'consultas'
      partitionKey: {
        paths: [
          '/paciente'
        ]
        kind: 'Hash'
      }
    }
  }
}

resource armazenamento 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'st${uniqueString(resourceGroup().id)}'
  location: localizacao
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: armazenamento
  name: 'default'
}

resource contDeploy 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobs
  name: 'deployments'
}

resource plano 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'plano-agendamento'
  location: localizacao
  sku: { name: 'FC1', tier: 'FlexConsumption' }
  kind: 'functionapp'
  properties: {
    reserved: true
  }
}

resource funcao 'Microsoft.Web/sites@2024-04-01' = {
  name: nomeFunction
  location: localizacao
  kind: 'functionapp,linux'
  dependsOn: [
    contDeploy
  ]
  properties: {
    serverFarmId: plano.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${armazenamento.properties.primaryEndpoints.blob}deployments'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          }
        }
      }
      scaleAndConcurrency: {
        instanceMemoryMB: 2048
        maximumInstanceCount: 1
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '8.0'
      }
    }
    siteConfig: {
      appSettings: [
        { name: 'AzureWebJobsStorage', value: 'DefaultEndpointsProtocol=https;AccountName=${armazenamento.name};AccountKey=${armazenamento.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}' }
        { name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING', value: 'DefaultEndpointsProtocol=https;AccountName=${armazenamento.name};AccountKey=${armazenamento.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}' }
        { name: 'CosmosEndpoint', value: conta.properties.documentEndpoint }
        { name: 'CosmosKey', value: conta.listKeys().primaryMasterKey }
      ]
    }
  }
}
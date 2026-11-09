@description('Nome da conta do Cosmos DB')
param nomeConta string = 'cosmosagendamento2026'

@description('Região dos recursos')
param localizacao string = resourceGroup().location

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
param baseName string
param location string
param sqlServerName string
param schoolContextDbName string

var uniqueName = '${baseName}${uniqueString(resourceGroup().id)}'

resource sqlServer 'Microsoft.Sql/servers@2021-05-01-preview' existing = {
  name: sqlServerName
}

resource schoolContextDb 'Microsoft.Sql/servers/databases@2021-05-01-preview' existing = {
  name: schoolContextDbName
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: baseName
  location: location
  sku: {
    name: 'D1'
    skuCapacity: {
      default: 1
    }
  }
  properties: {
    reserved: true
  }
  kind: 'linux'
}

resource website 'Microsoft.Web/sites@2021-02-01' = {
  name: uniqueName
  location: location
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      connectionStrings: [
        {
          name: 'SchoolContext'
          connectionString: 'Data Source=${sqlServer.properties.fullyQualifiedDomainName}:1433;Database=${schoolContextDb.name};MultipleActiveResultSets=true;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

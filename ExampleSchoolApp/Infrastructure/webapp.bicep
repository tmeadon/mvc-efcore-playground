param baseName string
param location string
param sqlServerName string
param schoolContextDbName string
param logWorkspaceName string

var uniqueName = '${baseName}${uniqueString(resourceGroup().id)}'

resource sqlServer 'Microsoft.Sql/servers@2021-05-01-preview' existing = {
  name: sqlServerName
}

resource schoolContextDb 'Microsoft.Sql/servers/databases@2021-05-01-preview' existing = {
  name: schoolContextDbName
}

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logWorkspaceName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: baseName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logWorkspace.id
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: baseName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
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
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
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

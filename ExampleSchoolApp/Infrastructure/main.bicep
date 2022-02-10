targetScope = 'subscription'

param baseName string = 'example-school-app'
param location string = 'northeurope'
param sqlAdminPassword string

@secure()
param sqlAdminUsername string

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: baseName
  location: location
}

module logs 'logs.bicep' = {
  scope: rg
  name: 'logs'
  params: {
    baseName: baseName
    location: location
  }
}

module sql 'sql.bicep' = {
  scope: rg
  name: 'sql'
  params: {
    baseName: baseName
    location: location
    sqlAdminPassword: sqlAdminPassword
    sqlAdminUsername: sqlAdminUsername
  }
}

module webApp 'webapp.bicep' = {
  scope: rg
  name: 'webapp'
  params: {
    baseName: baseName
    location: location
    schoolContextDbName: sql.outputs.schoolContextDbName
    sqlServerName: sql.outputs.serverName
    logWorkspaceName: logs.outputs.workspaceName
  }
}

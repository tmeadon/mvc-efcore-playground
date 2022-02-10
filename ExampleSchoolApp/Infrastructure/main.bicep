targetScope = 'subscription'

param baseName string = 'ExampleSchoolApp'
param location string = 'uksouth'
param sqlAdminPassword string

@secure()
param sqlAdminUsername string

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: baseName
  location: location
}

module sql 'sql.bicep' = {
  scope: rg
  name: 'sql'
  params: {
    baseName: baseName
    location: location
    sqlAdminPassword: sqlAdminUsername
    sqlAdminUsername: sqlAdminPassword
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
  }
}

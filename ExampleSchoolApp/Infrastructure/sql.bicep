param baseName string
param location string
param sqlAdminUsername string

@secure()
param sqlAdminPassword string

var uniqueName = '${baseName}${uniqueString(resourceGroup().id)}'

resource server 'Microsoft.Sql/servers@2021-05-01-preview' = {
  name: uniqueName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
  }
}

resource schoolContextDb 'Microsoft.Sql/servers/databases@2021-05-01-preview' = {
  parent: server
  name: 'SchoolContext'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 1073741824
  }
}

output serverName string = server.name
output schoolContextDbName string = schoolContextDb.name


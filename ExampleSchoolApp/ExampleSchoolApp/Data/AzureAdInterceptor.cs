using System.Data.Common;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExampleSchoolApp.Data;

public class AzureAdInterceptor : DbConnectionInterceptor
{
    private static readonly string[] _azureSqlScopes = new[]
    {
        "https://database.windows.net//.default"
    };

    private TokenRequestContext _azureSqlTokenRequestContext => new TokenRequestContext(_azureSqlScopes);

    private static readonly TokenCredential _credential = new DefaultAzureCredential();

    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData,
        InterceptionResult result)
    {
        var sqlConnection = connection as SqlConnection;
        
        if (DoesConnectionNeedAccessToken(sqlConnection))
        {
            var token = _credential.GetToken(_azureSqlTokenRequestContext, default);
            sqlConnection.AccessToken = token.Token;
        }
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection,
        ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        var sqlConnection = connection as SqlConnection;

        if (DoesConnectionNeedAccessToken(sqlConnection))
        {
            var token = await _credential.GetTokenAsync(_azureSqlTokenRequestContext, cancellationToken);
            sqlConnection.AccessToken = token.Token;
        }

        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private static bool DoesConnectionNeedAccessToken(SqlConnection connection)
    {
        // only Azure SQL connections need access tokens
        var connStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
        return connection.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrEmpty(connStringBuilder.UserID);
    }
}
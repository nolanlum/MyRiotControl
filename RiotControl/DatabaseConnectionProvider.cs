using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Npgsql;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace RiotControl
{
	class DatabaseConnectionProvider
	{
		DatabaseConfiguration ProviderConfiguration;

		public DatabaseConnectionProvider(DatabaseConfiguration configuration)
		{
			ProviderConfiguration = configuration;
		}

		public DbConnection GetConnection()
		{
			DbConnection connection;

			DbConnectionStringBuilder sb = new DbConnectionStringBuilder();
			sb.Add("Server", ProviderConfiguration.Host);
			sb.Add("Port", ProviderConfiguration.Port);
			sb.Add("User Id", ProviderConfiguration.Username);
			sb.Add("Database", ProviderConfiguration.Database);
			sb.Add("Pooling", true);
			sb.Add("Min Pool Size", ProviderConfiguration.MinimumPoolSize);
			sb.Add("Max Pool Size", ProviderConfiguration.MaximumPoolSize);

			switch (ProviderConfiguration.Provider.ToLower())
			{
				case "mysql":
					connection = new MySqlConnection(sb.ToString());

					break;
				case "postgre":
				default:
					// To ensure compatibility, an unspecified provider is considered "postgre".
					sb.Add("Preload reader", true);
					connection = new NpgsqlConnection(sb.ToString());

					break;
			}

			connection.Open();
			return connection;
		}

		public static DbCommand GetCommand(string query, DbConnection connection)
		{
			MySqlConnection mysqlConn = connection as MySqlConnection;
			if(mysqlConn != null)
				return new MySqlCommand(query, mysqlConn);

			// To ensure compatibility, an unspecified provider is considered "postgre".
			return new NpgsqlCommand(query, connection as NpgsqlConnection);
		}
	}
}

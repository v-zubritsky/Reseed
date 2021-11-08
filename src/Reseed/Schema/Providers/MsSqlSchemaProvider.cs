﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Reseed.Graphs;
using Testing.Common.Api.Schema;

namespace Reseed.Schema.Providers
{
	internal static class MsSqlSchemaProvider
	{
		public static IReadOnlyCollection<TableSchema> LoadSchema([NotNull] string connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			using var connection = new SqlConnection(connectionString);
			connection.Open();
			var tables = MsSqlSchemaReader.LoadTables(connection);
			if (tables.Count == 0)
			{
				return Array.Empty<TableSchema>();
			}
			else
			{
				var foreignKeys = MsSqlSchemaReader.LoadForeignKeys(connection, tables);
				return NodeBuilder<TableSchema>.CollectNodes(
					tables,
					foreignKeys,
					(r, t) => r.Map(_ => t),
					CreateTableSchema);
			}
		}

		private static TableSchema CreateTableSchema(
			TableData table,
			Reference<TableSchema>[] references) =>
			new(table.Name,
				table.Columns,
				table.PrimaryKey,
				references);
	}
}
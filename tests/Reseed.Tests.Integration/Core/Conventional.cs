﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Reseed.Data;
using Reseed.Dsl;

namespace Reseed.Tests.Integration.Core
{
	public static class Conventional
	{
		public static async Task<SqlServerContainer> CreateConventionalDatabase(TestFixtureBase fixture)
		{
			var scriptsFolder = GetDataFolder(fixture);
			var database = new SqlServerContainer(
				scriptsFolder,
				GetTestDataFileFilter("sql"));
			await database.StartAsync();
			
			return database;
		}

		public static IDataProvider CreateConventionalDataProvider(TestFixtureBase fixture) =>
			DataProvider.Xml(GetDataFolder(fixture), GetTestDataFileFilter("xml"));

		public static async Task AssertSeedSucceeds(
			TestFixtureBase fixture,
			RenderMode reseederRenderMode,
			Func<SqlEngine, Task> assertDataInserted,
			Func<SqlEngine, Task> assertDataDeleted)
		{
			await using var database = await CreateConventionalDatabase(fixture);
			var reseeder = new Reseeder(database.ConnectionString);
			var sqlEngine = new SqlEngine(database.ConnectionString);
			var dbActions = reseeder.Generate(
				reseederRenderMode,
				CreateConventionalDataProvider(fixture));

			reseeder.Execute(dbActions.PrepareDatabase);
			
			reseeder.Execute(dbActions.InsertData);
			await assertDataInserted(sqlEngine);

			reseeder.Execute(dbActions.DeleteData);
			await assertDataDeleted(sqlEngine);

			reseeder.Execute(dbActions.CleanupDatabase);
		}

		private static Func<string, bool> GetTestDataFileFilter(
			string fileExtension)
		{
			var testName = TestContext.CurrentContext.Test.MethodName;
			return s => Regex.IsMatch(s, $"(\\d+_)?{testName}.{fileExtension}");
		}

		private static string GetDataFolder(TestFixtureBase fixture) => Path.Combine(
			"Data",
			fixture.GetType().Name);
	}
}
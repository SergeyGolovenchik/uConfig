using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using uConfig.Configuration;
using uConfig.Data;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace uConfig;
public static class Extensions
{
	public static WebApplicationBuilder AddUConfig(this WebApplicationBuilder builder)
	{
		var connectionString = GetUmbracoConnectionString(builder, out var providerName);

		if (string.IsNullOrEmpty(connectionString) && string.IsNullOrWhiteSpace(providerName))
		{
			return builder; // DB connection not found. May happen on the first run of an app.
		}

		if (providerName == Umbraco.Cms.Persistence.EFCore.Constants.ProviderNames.SQLLite || providerName == "System.Data.SQLite")
		{
			return builder.AddUConfig(o => o.UseSqlite(connectionString));
		}

		if (providerName == Umbraco.Cms.Persistence.EFCore.Constants.ProviderNames.SQLServer || providerName == "System.Data.SqlClient")
		{
			return builder.AddUConfig(o => o.UseSqlServer(connectionString));
		}

		throw new NotSupportedException($"Db Provider {providerName} is not supported by this module.");
	}

	#region Private helpers

	/// <summary>
	/// Get's Umbraco Connection string and provider name even in case of local db and missing config value
	/// </summary>
	/// <param name="builder">Web app builder</param>
	/// <param name="providerName">Connection provider name</param>
	/// <returns>Connection string and provider name</returns>
	private static string? GetUmbracoConnectionString(WebApplicationBuilder builder, out string? providerName)
	{
		var connectionString = builder.Configuration.GetUmbracoConnectionString(out providerName);

		// If connection string was not returned properly - try get it from templating
		if (string.IsNullOrEmpty(connectionString))
		{
			var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
			if (!string.IsNullOrEmpty(dataDir))
			{
				if (File.Exists($"{dataDir}\\{uConfig.Constants.Data.DefaultSqliteFileName}"))   // Using development SQLite
				{
					connectionString = $"Data Source={dataDir}\\{uConfig.Constants.Data.DefaultSqliteFileName};Cache=Shared;Foreign Keys=True;Pooling=True";
					providerName = Umbraco.Cms.Persistence.EFCore.Constants.ProviderNames.SQLLite;
				}
				if (File.Exists($"{dataDir}\\{uConfig.Constants.Data.DefaultLocalDbFileName}"))         // Using development LocalDB
				{
					connectionString = $"Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename={dataDir}\\{uConfig.Constants.Data.DefaultLocalDbFileName};Integrated Security=True";
					providerName = Umbraco.Cms.Persistence.EFCore.Constants.ProviderNames.SQLServer;
				}
			}
		}

		return connectionString;
	}

	private static WebApplicationBuilder AddUConfig(this WebApplicationBuilder builder, Action<DbContextOptionsBuilder> dbContextOptions)
	{
		return builder.AddConfigurationSources(dbContextOptions)
					  .AddUmbracoDbSettingsContext(dbContextOptions);
	}

	/// <summary>
	/// Adds Sql configuration source and package settings
	/// </summary>
	/// <param name="builder">WebApp builder</param>
	/// <param name="dbContextOptions">Context options</param>
	/// <returns>WebApp builder</returns>
	private static WebApplicationBuilder AddConfigurationSources(this WebApplicationBuilder builder, Action<DbContextOptionsBuilder> dbContextOptions)
	{
		builder.Configuration.Sources.Add(new SqlConfigurationSource(dbContextOptions));
		builder.Configuration.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uConfig.Constants.PluginConfiguration.FileName), optional: true);

		return builder;
	}

	/// <summary>
	/// Adds DbContext as a service to DI
	/// </summary>
	/// <param name="builder">WebApp builder</param>
	/// <param name="dbContextOptions">Context options</param>
	/// <returns>WebApp builder</returns>
	private static WebApplicationBuilder AddUmbracoDbSettingsContext(this WebApplicationBuilder builder, Action<DbContextOptionsBuilder> dbContextOptions)
	{
		builder.Services.AddUmbracoDbContext<uConfig.Data.DbContext>(dbContextOptions);

		return builder;
	}
	#endregion

	#region Internal helpers
	internal static string? GetAssemblyCopyright(this Assembly assembly)
	{
		var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
		var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
		if (!string.IsNullOrEmpty(copyright) && !string.IsNullOrEmpty(company))
		{
			return copyright.Replace(company, $"<a href='{uConfig.Constants.PluginHomepage}' target='_blank'>{company}</a>");
		}

		return copyright;
	}

	internal static string GetLocalizedString(this ILocalizedTextService textService, string alias, string? defaultValue = null)
	{
		var result = textService.Localize(uConfig.Constants.Localization.Area, alias);

		if (result.Equals("[" + alias + "]"))
		{
			result = !string.IsNullOrEmpty(defaultValue) ? defaultValue : alias;
		}

		return result;
	}

	internal static BasicResponse GetLocalizedInfoResponse(this ILocalizedTextService textService, string alias, string defaultValue = "") => BasicResponse.Ok(textService.GetLocalizedString(alias, defaultValue));

	internal static dynamic GetLocalizedWarningResponse(this ILocalizedTextService textService, string alias, string defaultValue = "") => BasicResponse.Warn(textService.GetLocalizedString(alias, defaultValue));

	internal static dynamic GetLocalizedErrorResponse(this ILocalizedTextService textService, string alias, string defaultValue = "") => BasicResponse.Error(textService.GetLocalizedString(alias, defaultValue));

	#endregion
}

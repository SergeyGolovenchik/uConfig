using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using uConfig.Data;

namespace uConfig.Configuration;
internal class SqlConfigurationProvider : ConfigurationProvider
{
	private readonly DbContextOptionsBuilder<uConfig.Data.DbContext> builder = new();

	public SqlConfigurationProvider(SqlConfigurationSource source)
	{
		source.OptionsAction(builder);
		this.EnsureSourceTableCreated();
	}

	public override void Load()
	{
		using var dbContext = new uConfig.Data.DbContext(builder.Options);
		base.Data = dbContext.Settings.ToDictionary<DbSetting, string, string?>(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
	}

	public override string ToString() => uConfig.Constants.Data.ProviderName;

	private void EnsureSourceTableCreated()
	{
		using var dbContext = new uConfig.Data.DbContext(builder.Options);
		try
		{
			if (dbContext.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator databaseCreator)
			{
				databaseCreator.CreateTables();
			}
		}
		catch { }
	}
}

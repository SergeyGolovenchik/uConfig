using Microsoft.EntityFrameworkCore;

namespace uConfig.Data;
public class DbContext(DbContextOptions<uConfig.Data.DbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
	public DbSet<DbSetting> Settings { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<DbSetting>(entity =>
		{
			entity.ToTable(uConfig.Constants.Data.TableName);
			entity.HasKey(e => e.Key);
			entity.Property(e => e.Key).HasColumnName(uConfig.Constants.Data.KeyColumnName);
			entity.Property(e => e.Value).HasColumnName(uConfig.Constants.Data.ValueColumnName);
		});
	}
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace uConfig.Configuration;
internal class SqlConfigurationSource(Action<DbContextOptionsBuilder> optionsAction) : IConfigurationSource
{
	public Action<DbContextOptionsBuilder> OptionsAction { get; private set; } = optionsAction;

	public IConfigurationProvider Build(IConfigurationBuilder builder)
	{
		return new SqlConfigurationProvider(this);
	}
}

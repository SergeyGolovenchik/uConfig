using Microsoft.Extensions.Configuration;
using uConfig.Data;

namespace uConfig.Configuration;
internal static class ConfigurationHelper
{
	/// <summary>
	/// Recursively transforms IConfiguration to list of KVPs
	/// </summary>
	/// <param name="configSection">Configuration section</param>
	internal static IEnumerable<DbSetting> Flatten(this IConfiguration configSection)
	{
		List<DbSetting> result = [];

		foreach (var section in configSection.GetChildren())
		{
			if (section.Value == null)
			{
				var subSectionContent = section.Flatten();
				result.AddRange(subSectionContent);
			}
			else
			{
				result.Add(new() { Key = section.Path, Value = section.Value });
			}
		}

		return result;
	}

	/// <summary>
	/// Returns index of last IConfigurationProvider that has specified property key
	/// </summary>
	/// <param name="providersList"></param>
	/// <param name="key"></param>
	internal static int GetLastIndexWithKeyDefined(this IEnumerable<IConfigurationProvider> providersList, string key)
	{
		var providersCount = providersList.Count();

		for (int i = providersCount - 1; i >= 0; i--)
		{
			if (providersList.ElementAt(i).TryGet(key, out _))
			{
				return i;
			}
		}

		return -1;
	}
}

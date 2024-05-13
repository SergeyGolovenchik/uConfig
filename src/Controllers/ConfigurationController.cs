using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using uConfig.Configuration;
using uConfig.Data;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace uConfig.Controllers;
[PluginController(uConfig.Constants.PluginName)]
[Authorize(Policy = AuthorizationPolicies.UserBelongsToUserGroupInRequest, Roles = Umbraco.Cms.Core.Constants.Security.AdminGroupAlias)]
public class ConfigurationController : UmbracoAuthorizedApiController
{
	private readonly IEFCoreScopeProvider<uConfig.Data.DbContext> _pluginSqlScopeProvider;
	private readonly SqlConfigurationProvider? _pluginSqlConfigurationProvider;
	private readonly PluginConfiguration _pluginConfiguration;
	private readonly ILogger<ConfigurationController> _logger;
	private readonly IConfigurationRoot _configurationRoot;
	private readonly ILocalizedTextService _textservice;

	public ConfigurationController(
		IConfiguration configuration,
		ILocalizedTextService textservice,
		ILogger<ConfigurationController> logger,
		IEFCoreScopeProvider<uConfig.Data.DbContext> dbSettingsScopeProvider)
	{
		_logger = logger;
		_textservice = textservice;
		_pluginSqlScopeProvider = dbSettingsScopeProvider;
		_configurationRoot = (IConfigurationRoot)configuration;
		_pluginConfiguration = configuration.GetSection(uConfig.Constants.PluginConfiguration.RootPath).Get<PluginConfiguration>() ?? new PluginConfiguration();
		_pluginSqlConfigurationProvider = _configurationRoot.Providers.LastOrDefault(p => p.ToString()!.Equals(uConfig.Constants.Data.ProviderName)) as SqlConfigurationProvider;
	}

	/// <summary>
	/// Returns plugin configuration
	/// </summary>
	/// <returns>JSON with configuration data</returns>
	[HttpGet]
	public IActionResult Settings()
	{
		var response = new
		{
			providers = _configurationRoot.Providers.Select(p => p.ToString()),
			uConfigIndex = _pluginSqlConfigurationProvider != null ? _configurationRoot.Providers.IndexOf(_pluginSqlConfigurationProvider) : -1,
			copyright = Assembly.GetExecutingAssembly()?.GetAssemblyCopyright()
		};
		return new JsonResult(response);
	}

	/// <summary>
	/// Returns actual configuration with applied filters
	/// </summary>
	/// <returns>JSON array with key-value pairs</returns>
	[HttpGet]
	public IActionResult Configuration()
	{
		var configurationValues = _configurationRoot.Flatten();

		if (_pluginConfiguration != null)
		{
			configurationValues = configurationValues
				.Where(x => _pluginConfiguration.KeySelected(x.Key))
				.Where(x => !_pluginConfiguration.KeyExcluded(x.Key));
		}

		var configWithProviderInfo = configurationValues.Select(v =>
		{
			var providerIndex = _configurationRoot.Providers.GetLastIndexWithKeyDefined(v.Key);
			var isProtected = _pluginConfiguration?.KeyProtected(v.Key) ?? false;
			var isReadonly = _pluginConfiguration?.KeyReadonly(v.Key) ?? false;
			var displayValue = _pluginConfiguration?.GetDisplayName(v.Key, v.Value) ?? v.Value;

			return new
			{
				key = v.Key,
				value = v.Value,
				displayValue,
				providerIndex,
				isProtected,
				isReadonly
			};
		});

		return new JsonResult(configWithProviderInfo);
	}

	/// <summary>
	/// Updates value in database and reloads configuration
	/// </summary>
	/// <param name="setting">Setting to update</param>
	/// <returns>User-friendly message</returns>
	[HttpPost]
	public async Task<IActionResult> Update([FromBody] DbSetting setting)
	{
		var message = string.Empty;
		using (var scope = _pluginSqlScopeProvider.CreateScope())
		{
			await scope.ExecuteWithContextAsync<Task>(async db =>
			{
				if (db.Settings.Any(x => x.Key == setting.Key))
				{
					message = uConfig.Constants.Localization.UpdatedMessageKey;
					await db.Settings.Where(x => x.Key == setting.Key).ExecuteUpdateAsync(s => s.SetProperty(c => c.Value, setting.Value));
				}
				else
				{
					message = uConfig.Constants.Localization.AddedMessageKey;
					await db.Settings.AddAsync(setting);
					await db.SaveChangesAsync();
				}
			});
			scope.Complete();
		}

		var response = _textservice.GetLocalizedInfoResponse(message);
		response.MessageReplacements.Add(uConfig.Constants.Replacements.Key, setting.Key);
		response.MessageReplacements.Add(uConfig.Constants.Replacements.Value, _pluginConfiguration?.GetDisplayName(setting.Key, setting.Value) ?? setting.Value);

		_configurationRoot.Reload();

		return new JsonResult(response);
	}

	/// <summary>
	/// Deletes value from database and reloads configuration
	/// </summary>
	/// <param name="setting">Setting to delete</param>
	/// <returns>User-friendly message</returns>
	[HttpPost]
	public async Task<IActionResult> Delete([FromBody] DbSetting setting)
	{
		using (var scope = _pluginSqlScopeProvider.CreateScope())
		{
			await scope.ExecuteWithContextAsync<Task>(async db =>
			{
				await db.Settings.Where(x => x.Key == setting.Key).ExecuteDeleteAsync();
			});
			scope.Complete();
		}
		_configurationRoot.Reload();

		var newVal = _configurationRoot.GetValue<string>(setting.Key);
		var messageSource = string.IsNullOrEmpty(newVal) ?
									uConfig.Constants.Localization.DeletedMessageKey :
									uConfig.Constants.Localization.RolledBackMessageKey;
		var response = _textservice.GetLocalizedInfoResponse(messageSource);

		response.MessageReplacements.Add(uConfig.Constants.Replacements.Key, setting.Key);
		response.MessageReplacements.Add(uConfig.Constants.Replacements.Value, _pluginConfiguration?.GetDisplayName(setting.Key, newVal) ?? string.Empty);

		return new JsonResult(response);
	}
}

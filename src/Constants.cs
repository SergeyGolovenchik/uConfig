namespace uConfig;
internal static class Constants
{
	public const string PluginName = "uConfig";
	public const string PluginHomepage = "https://www.falcons.tech/products/uconfig";

	public static class Data
	{
		public const string ProviderName = uConfig.Constants.PluginName;
		public const string TableName = uConfig.Constants.PluginName;
		public const string KeyColumnName = "Key";
		public const string ValueColumnName = "Value";
		public const string DefaultSqliteFileName = "Umbraco.sqlite.db";
		public const string DefaultLocalDbFileName = "Umbraco.mdf";
	}

	public static class Localization
	{
		public const string Area = uConfig.Constants.PluginName;
		public const string UpdatedMessageKey = "editorSuccessfullyUpdated";
		public const string AddedMessageKey = "editorSuccessfullyAdded";
		public const string DeletedMessageKey = "editorSuccessfullyDeleted";
		public const string RolledBackMessageKey = "editorSuccessfullyRolledBack";
	}

	public static class PluginConfiguration
	{
		public const string FileName = "uConfigSettings.json";
		public const string RootPath = "Umbraco:uConfig";
	}

	public static class Replacements
	{
		public const string Key = "{key}";
		public const string Value = "{value}";
		public const char MaskedCharacter = '•';
	}
}

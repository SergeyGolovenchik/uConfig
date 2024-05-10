using System.Text.RegularExpressions;

namespace uConfig.Configuration;
internal class PluginConfiguration
{
	/// <summary>
	/// List of regexes for keys to be shown
	/// </summary>
	public List<string> Selected { get; set; } = new();

	/// <summary>
	/// List of regexes for keys to be excluded
	/// </summary>
	public List<string> Excluded { get; set; } = new();

	/// <summary>
	/// List of regexes for keys to be value-protected for displaying
	/// </summary>
	public List<string> Protected { get; set; } = new();

	/// <summary>
	/// List of regexes for keys to be readonly
	/// </summary>
	public List<string> Readonly { get; set; } = new();


	#region Helpers
	/// <summary>
	/// Indicates if key matches any of regexes from Selected list
	/// </summary>
	/// <param name="key">Configuration key</param>
	internal bool KeySelected(string key)
	{
		return this.Selected.Count() == 0 || this.Selected.Any(s => new Regex(s).IsMatch(key));
	}

	/// <summary>
	/// Indicates if key matches any of regexes from Excluded list
	/// </summary>
	/// <param name="key">Configuration key</param>
	internal bool KeyExcluded(string key)
	{
		return this.Excluded.Any(e => new Regex(e).IsMatch(key));
	}

	/// <summary>
	/// Indicates if key matches any of regexes from Protected list
	/// </summary>
	/// <param name="key">Configuration key</param>
	internal bool KeyProtected(string key)
	{
		return this.Protected.Any(p => new Regex(p).IsMatch(key));
	}

	/// <summary>
	/// Indicates if key matches any of regexes from Readonly list
	/// </summary>
	/// <param name="key">Configuration key</param>
	internal bool KeyReadonly(string key)
	{
		return this.Readonly.Any(r => new Regex(r).IsMatch(key));
	}

	/// <summary>
	/// Returns masked string for case when value is protected
	/// </summary>
	/// <param name="key">Configuration key</param>
	/// <param name="value">Original Value</param>
	/// <returns>Masked or original value</returns>
	internal string GetDisplayName(string key, string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return this.KeyProtected(key) ? new string(uConfig.Constants.Replacements.MaskedCharacter, value.Length) : value;
	}
	#endregion
}

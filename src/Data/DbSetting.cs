namespace uConfig.Data;
public record DbSetting
{
	public string Key { get; set; }
	public string Value { get; set; }

	public DbSetting() { }
	public DbSetting(string key, string value)
	{
		this.Key = key;
		this.Value = value;
	}
}

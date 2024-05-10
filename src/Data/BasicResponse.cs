namespace uConfig.Data;
public record BasicResponse
{
	public string Status { get; set; } = ResponseStatus.Ok.ToString();

	public string Message { get; set; } = string.Empty;

	public Dictionary<string, string> MessageReplacements { get; set; } = new();


	#region Helpers
	internal static BasicResponse Ok(string message) => new BasicResponse() { Status = ResponseStatus.Ok.ToString(), Message = message };

	internal static BasicResponse Warn(string message) => new BasicResponse() { Status = ResponseStatus.Warning.ToString(), Message = message };

	internal static BasicResponse Error(string message) => new BasicResponse() { Status = ResponseStatus.Error.ToString(), Message = message };
	#endregion
}

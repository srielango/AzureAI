namespace AzureAI.Domain.Configuration;
public class AISettingsOption
{
    public const string Name = "AISettings";
    public string ServiceEndPoint {  get; set; } = string.Empty;
    public string ServiceKey {  get; set; } = string.Empty;
}

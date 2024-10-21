namespace AzureAI.Application.AzureAI.Queries.FindLanguages;
public record FindLanguagesResponse
{
    public List<ResponseDocument> Documents { get; set; } = new List<ResponseDocument>();
    public string ModelVersion { get; set; } = string.Empty;
}

public record ResponseDocument(int Id, DetectedLanguage DetectedLanguage);

public record DetectedLanguage (string Name, string Iso6391Name, decimal confidenceScore);

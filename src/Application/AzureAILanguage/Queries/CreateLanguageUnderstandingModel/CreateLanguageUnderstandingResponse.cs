namespace AzureAI.Application.AzureAILanguage.Queries.CreateLanguageUnderstandingModel;
public class CreateLanguageUnderstandingResponse
{
    public string UserInput {  get; set; } = string.Empty;
    public string ConversationalTaskResult { get; set; } = string.Empty;
    public string TopIntent { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
}

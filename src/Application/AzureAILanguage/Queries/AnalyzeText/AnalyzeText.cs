using Azure;
using Azure.AI.TextAnalytics;
using AzureAI.Application.Common.Interfaces;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureAI.Application.AzureAILanguage.Queries.AnalyzeText;

public record AnalyzeTextQuery : IRequest<AnalyzeTextResponse>
{
    public string? InputText { get; set; }
}

public class AnalyzeTextQueryValidator : AbstractValidator<AnalyzeTextQuery>
{
    public AnalyzeTextQueryValidator()
    {
    }
}

public class AnalyzeTextQueryHandler : IRequestHandler<AnalyzeTextQuery, AnalyzeTextResponse>
{
    private readonly AISettingsOption _aISettingsOption;
    private readonly ILogger<AnalyzeTextQueryHandler> _logger;
    private readonly IFileHandler _fileHandler;
    public AnalyzeTextQueryHandler(IOptions<AISettingsOption> options, 
        ILogger<AnalyzeTextQueryHandler> logger, 
        IFileHandler fileHandler)
    {
        _aISettingsOption = options.Value;
        _logger = logger;
        _fileHandler = fileHandler;
    }

    public async Task<AnalyzeTextResponse> Handle(AnalyzeTextQuery request, CancellationToken cancellationToken)
    {
        var response = new AnalyzeTextResponse();

        var credentials = new AzureKeyCredential(_aISettingsOption.ServiceKey);
        var endPoint = new Uri(_aISettingsOption.ServiceEndPoint);
        TextAnalyticsClient aiClient = new TextAnalyticsClient(endPoint, credentials);

        if (request.InputText != null)
        {
            response.DocumentDetails.Add(await Analyze(aiClient, request.InputText));
        }
        else
        {
            foreach (var text in _fileHandler.GetTextFromFiles())
            {
                response.DocumentDetails.Add(await Analyze(aiClient, text));
            }
        }

        return response;
    }

    private async Task<DocumentDetails> Analyze(TextAnalyticsClient aiClient, string text)
    {
        var documentDetails = new DocumentDetails();

        documentDetails.OriginalText = text;

        documentDetails.DetectedLanguage = await DetectLanguage(aiClient, text);

        documentDetails.Sentiment = await GetSentiment(aiClient, text);

        documentDetails.KeyPhrases = await ExtractKeyPhrases(aiClient, text);

        documentDetails.Entities = await GetEntities(aiClient, text);

        documentDetails.LinkedEntities = await GetLinkedEntities(aiClient, text);

        return documentDetails;
    }

    private async Task<List<LinkedEntity>> GetLinkedEntities(TextAnalyticsClient aiClient, string text)
    {
        List<LinkedEntity> entities = new List<LinkedEntity>();
        LinkedEntityCollection linkedEntities = await aiClient.RecognizeLinkedEntitiesAsync(text);
        if (!linkedEntities.Any())
        {
            entities.AddRange(linkedEntities);
        }
        return entities;
    }


    private async Task<List<CategorizedEntity>> GetEntities(TextAnalyticsClient aiClient, string text)
    {
        List<CategorizedEntity> entites = new();

        CategorizedEntityCollection extractedEntities = await aiClient.RecognizeEntitiesAsync(text);
        if (extractedEntities.Any())
        {
            entites.AddRange(extractedEntities);
        }
        return entites;
    }

    private async Task<List<string>> ExtractKeyPhrases(TextAnalyticsClient aiClient, string text)
    {
        List<string> ExtractKeyPhrases = new List<string>();

        KeyPhraseCollection phrases = await aiClient.ExtractKeyPhrasesAsync(text);
        if (phrases.Count > 0)
        {
            ExtractKeyPhrases.AddRange(phrases);
        }

        return ExtractKeyPhrases;
    }

    private async Task<string> GetSentiment(TextAnalyticsClient aiClient, string text)
    {
        DocumentSentiment sentiment = await aiClient.AnalyzeSentimentAsync(text);
        return sentiment.Sentiment.ToString();
    }

    private async Task<string> DetectLanguage(TextAnalyticsClient aiClient, string text)
    {
        DetectedLanguage detectedLanguage = await aiClient.DetectLanguageAsync(text);
        return detectedLanguage.Name;
    }
}

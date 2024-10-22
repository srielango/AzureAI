using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AzureAI.Application.Common.Interfaces;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureAI.Application.AzureAI.Queries.FindLanguages;

public record FindLanguagesQuery : IRequest<FindLanguagesResponse>
{
    public required string InputText { get; set; }
}

public record DocumentList (List<Document> Documents);
public record Document(int Id, string Text);

public class FindLanguagesQueryValidator : AbstractValidator<FindLanguagesQuery>
{
    public FindLanguagesQueryValidator()
    {
    }
}

public class FindLanguagesQueryHandler : IRequestHandler<FindLanguagesQuery, FindLanguagesResponse>
{
    private readonly AISettingsOption _aISettingsOption;
    private readonly IAzureAIClient _azureAIClient;
    private readonly ILogger<FindLanguagesQueryHandler> _logger;

    public FindLanguagesQueryHandler(IOptions<AISettingsOption> options, IAzureAIClient azureAIClient, ILogger<FindLanguagesQueryHandler> logger)
    {
        _aISettingsOption = options.Value;
        _azureAIClient = azureAIClient;
        _logger = logger;
    }

    public async Task<FindLanguagesResponse> Handle(FindLanguagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var documents = new DocumentList(new List<Document>
            {
                new Document(1, request.InputText)
            });

            var json = JsonSerializer.Serialize(documents);

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Ocp-Apim-Subscription-Key", _aISettingsOption.ServiceKey }
            };

            using var response = await _azureAIClient.FindLanguage(json, headers);
            var responseDocument = await response.Content.ReadFromJsonAsync<FindLanguagesResponse>();

            return responseDocument ?? new FindLanguagesResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred in FindLanguagesQueryHandler. {ex}");
            throw new Exception("Error occurred in FindLanguagesQueryHandler", ex);
        }
    }
}

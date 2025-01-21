using System.ClientModel;
using Azure.AI.OpenAI;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AzureAI.Application.AzureOpenAI.Queries.UseOpenAI;
public class OpenAIChatQuery : IRequest<OpenAIChatResponse>
{
    public string SystemMessage { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
}

public class OpenAIChatQueryValidator : AbstractValidator<OpenAIChatQuery>
{
    public OpenAIChatQueryValidator()
    {

    }
}

public class OpenAIChatQueryHandler : IRequestHandler<OpenAIChatQuery, OpenAIChatResponse>
{
    private readonly AISettingsOption _aISettingsOption;
    private readonly ILogger<OpenAIChatQueryHandler> _logger;

    public OpenAIChatQueryHandler(IOptions<AISettingsOption> options,
        ILogger<OpenAIChatQueryHandler> logger)
    {
        _aISettingsOption = options.Value;
        _logger = logger;
    }

    public async Task<OpenAIChatResponse> Handle(OpenAIChatQuery request, CancellationToken cancellationToken)
    {
        var oaiEndPoint = _aISettingsOption.OaiEndPoint;
        var oaiKey = _aISettingsOption.OaiKey;
        var oaiDeployment = _aISettingsOption.OaiDeployment;

        AzureOpenAIClient azureClient = new(new Uri(oaiEndPoint), new ApiKeyCredential(oaiKey));
        ChatClient chatClient = azureClient.GetChatClient(oaiDeployment);

        // Create a request with System and User messages
        var chatCompletionsOptions = new ChatCompletionOptions()
        {
            Temperature = 0.9f, // Set the desired temperature (range: 0-1)
            MaxOutputTokenCount = 150
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(request.SystemMessage),
                new UserChatMessage(request.UserMessage)
            ], chatCompletionsOptions);

        // Get response from Azure OpenAI
        
        Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");

        return new OpenAIChatResponse()
        {
            ChatResponse = $"{completion.Role}: {completion.Content[0].Text}"
        };
    }
}

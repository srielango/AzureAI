using Azure;
using Azure.AI.Language.QuestionAnswering;
using AzureAI.Application.AzureAILanguage.Queries.QnA;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureAI.Application.AzureAILanguage.Queries.CreateQnA;

public record QnAQuery : IRequest<QnAResponse>
{
    public string Question {  get; set; } = string.Empty;
}

public class CreateQnAQueryValidator : AbstractValidator<QnAQuery>
{
    public CreateQnAQueryValidator()
    {
    }
}

public class CreateQnAQueryHandler : IRequestHandler<QnAQuery, QnAResponse>
{

    private readonly AISettingsOption _aISettingsOption;
    private readonly ILogger<CreateQnAQueryHandler> _logger;

    public CreateQnAQueryHandler(IOptions<AISettingsOption> options, 
        ILogger<CreateQnAQueryHandler> logger)
    {
        _aISettingsOption = options.Value;
        _logger = logger;
    }

    public async Task<QnAResponse> Handle(QnAQuery request, CancellationToken cancellationToken)
    {
        string projectName = "LearnFAQ";
        string deploymentName = "production";

        var credentials = new AzureKeyCredential(_aISettingsOption.ServiceKey);
        var endPoint = new Uri(_aISettingsOption.ServiceEndPoint);
        var aiClient = new QuestionAnsweringClient(endPoint, credentials);

        QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
        Response<AnswersResult> response = await aiClient.GetAnswersAsync(request.Question, project);

        var qnAResponse = new QnAResponse();
        qnAResponse.Answers = response.Value.Answers.ToList();
        return qnAResponse;
    }
}

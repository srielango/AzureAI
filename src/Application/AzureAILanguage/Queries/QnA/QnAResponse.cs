using Azure.AI.Language.QuestionAnswering;

namespace AzureAI.Application.AzureAILanguage.Queries.QnA;
public class QnAResponse
{
    public List<KnowledgeBaseAnswer> Answers { get; set; } = new();
}

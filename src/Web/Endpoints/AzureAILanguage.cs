
using AzureAI.Application.AzureAILanguage.Queries.AnalyzeText;

namespace AzureAI.Web.Endpoints;

public class AzureAILanguage : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(AnalyzeText, "TextAnalyzer");
    }

    public Task<AnalyzeTextResponse> AnalyzeText(ISender sender, [AsParameters] AnalyzeTextQuery query)
    {
        return sender.Send(query);
    }
}

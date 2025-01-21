using AzureAI.Application.AzureOpenAI.Queries.UseOpenAI;

namespace AzureAI.Web.Endpoints;

public class AzureOpenAI : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(OpenAIChat);
    }

    public Task<OpenAIChatResponse> OpenAIChat(ISender sender, [AsParameters] OpenAIChatQuery query)
    {
        return sender.Send(query);
    }
}

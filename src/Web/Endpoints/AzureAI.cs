
using AzureAI.Application.AzureAI.Queries.FindLanguages;

namespace AzureAI.Web.Endpoints;

public class AzureAI : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(FindLanguage);
    }

    public Task<FindLanguagesResponse> FindLanguage(ISender sender, [AsParameters] FindLanguagesQuery query)
    {
        return sender.Send(query);
    }
}

﻿
using AzureAI.Application.AzureAILanguage.Queries.AnalyzeText;
using AzureAI.Application.AzureAILanguage.Queries.CreateLanguageUnderstandingModel;
using AzureAI.Application.AzureAILanguage.Queries.CreateQnA;
using AzureAI.Application.AzureAILanguage.Queries.QnA;

namespace AzureAI.Web.Endpoints;

public class AzureAILanguage : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(AnalyzeText, "TextAnalyzer")
            .MapGet(QnA, "QnA")
            .MapGet(LanguageUnderstanding, "LanguageUnderstanding")
            ;
    }

    public Task<AnalyzeTextResponse> AnalyzeText(ISender sender, [AsParameters] AnalyzeTextQuery query)
    {
        return sender.Send(query);
    }

    public Task<QnAResponse> QnA(ISender sender, [AsParameters] QnAQuery query)
    {
        return sender.Send(query);
    }

    public Task<CreateLanguageUnderstandingResponse> LanguageUnderstanding(ISender sender, [AsParameters] CreateLanguageUnderstandingModelQuery query)
    {
        return sender.Send(query);
    }
}

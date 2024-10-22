using Refit;

namespace AzureAI.Application.Common.Interfaces;
[Headers("accept: application/json")]
public interface IAzureAIClient
{
    [Post("/text/analytics/v3.1/languages?")]
    Task<HttpResponseMessage> FindLanguage([Body] string s, [HeaderCollection] IDictionary<string, string> headers);
}

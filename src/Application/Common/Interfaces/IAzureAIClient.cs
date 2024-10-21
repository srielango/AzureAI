using Refit;

namespace AzureAI.Application.Common.Interfaces;
[Headers("accept: application/json")]
public interface IAzureAIClient
{
    [Headers("Content-Type: application/json;charset=utf-8")]
    [Post("/text/analytics/v3.1/languages?")]
    Task<string> FindLanguage([Body] string s);
}

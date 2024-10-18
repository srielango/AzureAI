using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using AzureAI.Application.Common.Interfaces;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AzureAI.Application.AzureAI.Queries.FindLanguages;

public record FindLanguagesQuery : IRequest<string>
{
    public required string InputText { get; set; }
}

public class FindLanguagesQueryValidator : AbstractValidator<FindLanguagesQuery>
{
    public FindLanguagesQueryValidator()
    {
    }
}

public class FindLanguagesQueryHandler : IRequestHandler<FindLanguagesQuery, string>
{
    private readonly AISettingsOption _aISettingsOption;

    public FindLanguagesQueryHandler(IOptions<AISettingsOption> options)
    {
        _aISettingsOption = options.Value;
    }

    public async Task<string> Handle(FindLanguagesQuery request, CancellationToken cancellationToken)
    {
        StringBuilder result = new StringBuilder();

        try
        {
            var jsonBody = new JObject(
                new JProperty("documents",
                new JArray(
                    new JObject(
                        new JProperty("id", 1),
                        new JProperty("text", request.InputText)
                        ))));

            var utf8 = new UTF8Encoding(true, true);
            var encodedBytes = utf8.GetBytes(jsonBody.ToString());

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _aISettingsOption.ServiceKey);

            var uri = _aISettingsOption.ServiceEndPoint + "text/analytics/v3.1/languages?" + queryString;

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(encodedBytes))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var results = JObject.Parse(responseContent);
                result.AppendLine(results.ToString());

                foreach(JObject document in results["documents"]!)
                {
                    result.AppendLine("\nLanguage: " + (string)document["detectedLanguage"]!["name"]!);
                }
            }
            else
            {
                result.AppendLine(response.ToString());
            }
            return result.ToString();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}

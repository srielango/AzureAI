using System.Net.Http.Json;
using System.Text;
using System.Web;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AzureAI.Application.AzureAI.Queries.FindLanguages;

public record FindLanguagesQuery : IRequest<FindLanguagesResponse>
{
    public required string InputText { get; set; }
}

public record DocumentList (List<Document> Documents);
public record Document(int Id, string Text);

public class FindLanguagesQueryValidator : AbstractValidator<FindLanguagesQuery>
{
    public FindLanguagesQueryValidator()
    {
    }
}

public class FindLanguagesQueryHandler : IRequestHandler<FindLanguagesQuery, FindLanguagesResponse>
{
    private readonly AISettingsOption _aISettingsOption;

    public FindLanguagesQueryHandler(IOptions<AISettingsOption> options)
    {
        _aISettingsOption = options.Value;
    }

    public async Task<FindLanguagesResponse> Handle(FindLanguagesQuery request, CancellationToken cancellationToken)
    {
        StringBuilder result = new StringBuilder();

        try
        {
            var documents = new DocumentList(new List<Document>()
            {
                new Document(1, request.InputText)
            });

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _aISettingsOption.ServiceKey);

            var uri = _aISettingsOption.ServiceEndPoint + "/text/analytics/v3.1/languages?" + queryString;

            using var response = await client.PostAsJsonAsync(uri, documents);

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

            var responseDocument = await response.Content.ReadFromJsonAsync<FindLanguagesResponse>();

            return responseDocument ?? new FindLanguagesResponse();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}

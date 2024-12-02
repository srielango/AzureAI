using System.Text.Json;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using Azure.Core.Serialization;
using AzureAI.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureAI.Application.AzureAILanguage.Queries.CreateLanguageUnderstandingModel;

public record CreateLanguageUnderstandingModelQuery : IRequest<CreateLanguageUnderstandingResponse>
{
    public string UserInput { get; set; } = string.Empty;
}

public class CreateLanguageUnderstandingModelQueryValidator : AbstractValidator<CreateLanguageUnderstandingModelQuery>
{
    public CreateLanguageUnderstandingModelQueryValidator()
    {
    }
}

public class CreateLanguageUnderstandingModelQueryHandler : IRequestHandler<CreateLanguageUnderstandingModelQuery, CreateLanguageUnderstandingResponse>
{
    private readonly AISettingsOption _aISettingsOption;
    private readonly ILogger<CreateLanguageUnderstandingModelQueryHandler> _logger;
    
    public CreateLanguageUnderstandingModelQueryHandler(IOptions<AISettingsOption> options, ILogger<CreateLanguageUnderstandingModelQueryHandler> logger)
    {
        _aISettingsOption = options.Value;
        _logger = logger;
    }

    
    public async Task<CreateLanguageUnderstandingResponse> Handle(CreateLanguageUnderstandingModelQuery request, CancellationToken cancellationToken)
    {
        var result = new CreateLanguageUnderstandingResponse();

        string predictionEndpoint = _aISettingsOption.ServiceEndPoint;
        string predictionKey = _aISettingsOption.ServiceKey;

        Uri uriEndPoint = new Uri(predictionEndpoint);
        AzureKeyCredential keyCredential = new AzureKeyCredential(predictionKey);

        ConversationAnalysisClient client = new ConversationAnalysisClient(uriEndPoint, keyCredential);

        var projectName = "Clock";
        var deploymentName = "production";
        var data = new
        {
            analysisInput = new
            {
                conversationItem = new
                {
                    text = request.UserInput,
                    id = "1",
                    participantId = "1",
                }
            },
            parameters = new
            {
                projectName,
                deploymentName,
                // Use Utf16CodeUnit for strings in .NET.
                stringIndexType = "Utf16CodeUnit",
            },
            kind = "Conversation",
        };

        // Send request
        Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data));
        dynamic conversationalTaskResult = response.Content.ToDynamicFromJson(JsonPropertyNames.CamelCase);
        dynamic conversationPrediction = conversationalTaskResult.Result.Prediction;
        var options = new JsonSerializerOptions { WriteIndented = true };

        result.ConversationalTaskResult = JsonSerializer.Serialize(conversationalTaskResult, options);
        result.UserInput = request.UserInput;

        if (conversationPrediction.Intents[0].ConfidenceScore > 0.5)
        {
            result.TopIntent = conversationPrediction.TopIntent;
        }

        switch (result.TopIntent)
        {
            case "GetTime":
                var location = "local";
                // Check for a location entity
                foreach (dynamic entity in conversationPrediction.Entities)
                {
                    if (entity.Category == "Location")
                    {
                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                        location = entity.Text;
                    }
                }
                // Get the time for the specified location
                result.Response = GetTime(location);
                break;
            case "GetDay":
                var date = DateTime.Today.ToShortDateString();
                // Check for a Date entity
                foreach (dynamic entity in conversationPrediction.Entities)
                {
                    if (entity.Category == "Date")
                    {
                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                        date = entity.Text;
                    }
                }
                // Get the day for the specified date
                result.Response = GetDay(date);
                break;
            case "GetDate":
                var day = DateTime.Today.DayOfWeek.ToString();
                // Check for entities            
                // Check for a Weekday entity
                foreach (dynamic entity in conversationPrediction.Entities)
                {
                    if (entity.Category == "Weekday")
                    {
                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                        day = entity.Text;
                    }
                }
                // Get the date for the specified day
                result.Response = GetDate(day);
                break;
            default:
                // Some other intent (for example, "None") was predicted
                result.Response = "Try asking me for the time, the day, or the date.";
                break;
        }

        return result;
    }

    static string GetTime(string location)
    {
        var timeString = "";
        var time = DateTime.Now;

        /* Note: To keep things simple, we'll ignore daylight savings time and support only a few cities.
           In a real app, you'd likely use a web service API (or write  more complex code!)
           Hopefully this simplified example is enough to get the the idea that you
           use LU to determine the intent and entities, then implement the appropriate logic */

        switch (location.ToLower())
        {
            case "local":
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "london":
                time = DateTime.UtcNow;
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "sydney":
                time = DateTime.UtcNow.AddHours(11);
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "new york":
                time = DateTime.UtcNow.AddHours(-5);
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "nairobi":
                time = DateTime.UtcNow.AddHours(3);
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "tokyo":
                time = DateTime.UtcNow.AddHours(9);
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            case "delhi":
                time = DateTime.UtcNow.AddHours(5.5);
                timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2");
                break;
            default:
                timeString = "I don't know what time it is in " + location;
                break;
        }

        return timeString;
    }

    static string GetDate(string day)
    {
        string date_string = "I can only determine dates for today or named days of the week.";

        // To keep things simple, assume the named day is in the current week (Sunday to Saturday)
        DayOfWeek weekDay;
        if (Enum.TryParse(day, true, out weekDay))
        {
            int weekDayNum = (int)weekDay;
            int todayNum = (int)DateTime.Today.DayOfWeek;
            int offset = weekDayNum - todayNum;
            date_string = DateTime.Today.AddDays(offset).ToShortDateString();
        }
        return date_string;

    }

    static string GetDay(string date)
    {
        // Note: To keep things simple, dates must be entered in US format (MM/DD/YYYY)
        string day_string = "Enter a date in MM/DD/YYYY format.";
        DateTime dateTime;
        if (DateTime.TryParse(date, out dateTime))
        {
            day_string = dateTime.DayOfWeek.ToString();
        }

        return day_string;
    }
}

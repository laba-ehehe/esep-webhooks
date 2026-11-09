using System.Text;
using Newtonsoft.Json;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>

public string FunctionHandler(object input, ILambdaContext context)
    {
        var issueEvent = Newtonsoft.Json.Linq.JObject.Parse(input.ToString()!);
        string? issueUrl = (string?)issueEvent["issue"]?["html_url"];

        if (string.IsNullOrEmpty(issueUrl))
            return "Ignored event without an issue link";

        string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL")
            ?? throw new InvalidOperationException("SLACK_URL is missing");

        string payload = JsonConvert.SerializeObject(new
        {
            text = $"Issue Created: {issueUrl}"
        });

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, slackUrl);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = client.Send(request);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}

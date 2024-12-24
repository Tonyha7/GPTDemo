using System.Text;
using GPTDemo;
using Newtonsoft.Json;

class Program
{
    private static readonly string apiKey = API.ApiKey;

    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var conversation = new Conversation();

        while (true)
        {
            Console.WriteLine("请输入您的消息（输入'/exit'退出）:");
            var input = Console.ReadLine();
            if (input?.ToLower() == "/exit") break;

            var message = new Message { Role = "user", Content = input };
            conversation.Messages.Add(message);

            var response = await SendMessage(client, conversation);
            Console.WriteLine($"GPT: {response}");
        }
    }

    static async Task<string> SendMessage(HttpClient client, Conversation conversation)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = conversation.Messages
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(API.ApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            Console.WriteLine($"Error Details: {errorContent}");
            return string.Empty;
        }

        var resString = await response.Content.ReadAsStringAsync();
        dynamic res = JsonConvert.DeserializeObject(resString);
        
        string assistantMsg = res.choices[0].message.content;
        conversation.Messages.Add(new Message { Role = "assistant", Content = assistantMsg });

        return assistantMsg;
    }
}

class Conversation
{
    public List<Message> Messages { get; set; } = new List<Message>();
}

class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}

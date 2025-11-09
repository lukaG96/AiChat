
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI;


var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// Config
var config = builder.Configuration;


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost",
                "http://nginx",
                "http://ai-chat-react"
            ) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Services
builder.Services.AddSignalR();
// builder.Services.AddSingleton<IChatClient>(sp =>
// {
//     var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

//     string? githubToken = config["AI:GitHubToken"] ?? "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";
//     string? modelName = config["AI:ModelName"] ?? "gpt-4o";

//     var openAIClient = new OpenAIClient(
//         new System.ClientModel.ApiKeyCredential(githubToken!),
//         new OpenAIClientOptions { Endpoint = new Uri("https://api.openai.com/v1/responses") });

//     var baseChatClient = openAIClient.GetChatClient(modelName).AsIChatClient();

//     return new ChatClientBuilder(baseChatClient)
//         .UseLogging(loggerFactory)
//         .UseFunctionInvocation()
//         .Build();
// });
builder.Services.AddChatClient(_ => 
        new OpenAI.Chat.ChatClient(
            "gpt-4o-mini", 
            "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"
            ).AsIChatClient()) 
            .UseFunctionInvocation(configure: x => 
                { 
                    x.IncludeDetailedErrors = true; 
                }
            );


// MCP client setup
builder.Services.AddSingleton(async sp =>
{
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("McpSetup");

    string? mcpDockerCmd = config["McpServerDockerCommand"] ?? "docker run -i --rm --name studentsmcp-server 066543543/studentsmcp:latest";
    var dockerCmdParts = mcpDockerCmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var dockerCommand = dockerCmdParts.First();
    var dockerArguments = dockerCmdParts.Skip(1).ToArray();

    var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "Students MCP Server (Docker)",
        Command = dockerCommand,
        Arguments = dockerArguments,
        WorkingDirectory = Directory.GetCurrentDirectory(),
    });

    var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
    logger.LogInformation("MCP client initialized.");
    return mcpClient;
});

builder.Services.AddSingleton<ChatHub>();

var app = builder.Build();
app.UseCors("AllowReactApp");

app.MapHub<ChatHub>("/chatHub");
app.MapHub<ChatHubStreaming>("/chatHubStreaming");

app.MapGet("/", () => "SignalR AI Chat Server running 🚀");

app.Run();
// using ModelContextProtocol.Client;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.AI;
// using System.Text.Json;
// using Microsoft.Extensions.Configuration;

// // Load configuration from appsettings.json
// var config = new ConfigurationBuilder()
//     .SetBasePath(Directory.GetCurrentDirectory())
//     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//     .Build();

// string? githubToken = config["AI:GitHubToken"];
// string? modelName = config["AI:ModelName"] ?? "gpt-4o";

// // MCP Client Transport using HTTP (Docker)
// string? mcpDockerCmd = config["McpServerDockerCommand"] ?? "docker run -i --rm melmasry/studentsmcp";
// // Split mcpDockerCmd into command and arguments
// var dockerCmdParts = mcpDockerCmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
// var dockerCommand = dockerCmdParts.First();
// var dockerArguments = dockerCmdParts.Skip(1).ToArray();

// var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
// {
//     Name = "Students MCP Server (Docker)",
//     Command = dockerCommand,
//     Arguments = dockerArguments,
//     WorkingDirectory = Directory.GetCurrentDirectory(),
// });

// // Logger
// using var loggerFactory = LoggerFactory.Create(builder =>
//     builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// // Create MCP Client
// var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

// // Get available tools from MCP Server
// var mcpTools = await mcpClient.ListToolsAsync();

// var toolsJson = JsonSerializer.Serialize(mcpTools, new JsonSerializerOptions { WriteIndented = true });
// Console.WriteLine("\nAvailable Tools:\n" + toolsJson);

// await Task.Delay(100);

// // Create OpenAI chat client for GitHub Models and convert to IChatClient
// var openAIClient = new OpenAI.OpenAIClient(
//     new System.ClientModel.ApiKeyCredential(githubToken!),
//     new OpenAI.OpenAIClientOptions
//     {
//         Endpoint = new Uri("https://models.inference.ai.azure.com")
//     });

// // Use the extension method to convert to Microsoft.Extensions.AI.IChatClient
// var baseChatClient = openAIClient.GetChatClient(modelName).AsIChatClient();
// var chatClient = new ChatClientBuilder(baseChatClient)
//     .UseLogging(loggerFactory)
//     .UseFunctionInvocation()
//     .Build();

// // Prompt loop
// Console.WriteLine("Type your message below (type 'exit' to quit):");

// while (true)
// {
//     Console.Write("\n You: ");
//     var userInput = Console.ReadLine();

//     if (string.IsNullOrWhiteSpace(userInput))
//         continue;

//     if (userInput.Trim().ToLower() == "exit")
//     {
//         Console.WriteLine("Exiting chat...");
//         break;
//     }

//     var messages = new List<ChatMessage> {
//         new(ChatRole.System, "You are a helpful assistant."),
//         new(ChatRole.User, userInput)
//     };

//     try
//     {
//         var response = await chatClient.GetResponseAsync(
//             messages,
//             new ChatOptions { Tools = mcpTools.ToArray<AITool>() });

//         var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);

//         if (assistantMessage != null)
//         {
//             var textOutput = string.Join($" ", assistantMessage.Contents.Select(c => c.ToString()));
//             Console.WriteLine("\n AI: " + textOutput);
//         }
//         else
//         {
//             Console.WriteLine("\n AI: (no assistant message received)");
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"\n Error: {ex.Message}");
//     }
// }

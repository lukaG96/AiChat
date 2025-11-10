
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OllamaSharp;


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

// OPEN AI CHAT CLIENT
// builder.Services.AddChatClient(_ => 
//         new OpenAI.Chat.ChatClient(
//             "gpt-4o-mini", 
//             "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"
//             ).AsIChatClient()) 
//             .UseFunctionInvocation(configure: x => 
//                 { 
//                     x.IncludeDetailedErrors = true; 
//                 }
//             );

// OLLAMA CHAT CLIENT
builder.Services.AddChatClient(_ =>
    new OllamaApiClient(new Uri("http://ollama:11434"), "llama3.1:8b"))
    .UseFunctionInvocation(configure: x =>
    {
        x.IncludeDetailedErrors = true;
    });




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



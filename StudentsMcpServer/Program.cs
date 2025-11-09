using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudentsMcpServer;
using StudentsMcpServer.Models;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to console and file
builder.Logging.ClearProviders();
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add file logging
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
Directory.CreateDirectory(logDirectory);
var logFilePath = Path.Combine(logDirectory, $"studentsmcp-{DateTime.Now:yyyyMMdd}.log");

builder.Logging.AddFile(logFilePath, minimumLevel: LogLevel.Information);

// Register StudentService
builder.Services.AddSingleton<StudentService>();

// Build MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var host = builder.Build();

// Initialize StudentTools with service and logger
var studentService = host.Services.GetRequiredService<StudentService>();
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("StudentTools");
StudentTools.Initialize(studentService, logger);

await host.RunAsync();

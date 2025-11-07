using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;

public class ChatHubStreaming : Hub
{
    private readonly IChatClient _chatClient;
    private readonly Task<IMcpClient> _mcpClientTask;

    public ChatHubStreaming(IChatClient chatClient, Task<IMcpClient> mcpClientTask)
    {
        _chatClient = chatClient;
        _mcpClientTask = mcpClientTask;
    }

    public async Task SendMessage(string user, string message)
    {
        var mcpClient = await _mcpClientTask;
        var mcpTools = await mcpClient.ListToolsAsync();

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant."),
            new(ChatRole.User, message)
        };

        try
        {
            var sb = new StringBuilder();

            // Stream directly
            await foreach (var update in _chatClient.GetStreamingResponseAsync(
                messages,
                new ChatOptions { Tools = mcpTools.ToArray<AITool>() }))
            {
                var delta = update?.ToString() ?? "";
                sb.Append(delta);

                // Send chunk immediately
                await Clients.Caller.SendAsync("ReceiveStreamChunk", "AI", delta);
            }

            // Send complete text
            await Clients.Caller.SendAsync("ReceiveStreamComplete", "AI", sb.ToString());

            // Confirm user message
            await Clients.Caller.SendAsync("ReceiveMessage", "You", message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error: {ex.Message}");
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

public class ChatHub : Hub
{
    private readonly IChatClient _chatClient;
    private readonly Task<IMcpClient> _mcpClientTask;

    public ChatHub(IChatClient chatClient, Task<IMcpClient> mcpClientTask)
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
            var response = await _chatClient.GetResponseAsync(messages, new ChatOptions
            {
                Tools = mcpTools.ToArray<AITool>()
            });

            var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
            string reply = assistantMessage != null
                ? string.Join(" ", assistantMessage.Contents.Select(c => c.ToString()))
                : "(no response)";

            await Clients.All.SendAsync("ReceiveMessage", "AI", reply);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error: {ex.Message}");
        }
    }
}

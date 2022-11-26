using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace WebSocketDemo.Sockets;

public class WebsocketClient
{
    public string Id { get; set; }
    public string RoomNo { get; set; }
    public WebSocket WebSocket { get; set; }
    public WebSocketReceiveResult ReceiveResult { get; set; }

    public async Task SendMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await WebSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, ReceiveResult.Count),
            ReceiveResult.MessageType, ReceiveResult.EndOfMessage, CancellationToken.None);
    }
}

public class Message
{
    [JsonProperty("sendClientId")]
    public string SendClientId { get; set; }

    [JsonProperty("nick")]
    public string Nick { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("msg")]
    public string Msg { get; set; }
}

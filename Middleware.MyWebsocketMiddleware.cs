using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using WebSocketDemo.Sockets;

namespace WebSocketDemo.Middleware;

/// <summary>
/// websocket 中间件
/// </summary>
public class MyWebsocketMiddleware 
{
    private ILogger<MyWebsocketMiddleware> _logger;
    private readonly RequestDelegate _next;

    public MyWebsocketMiddleware(ILogger<MyWebsocketMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                // 接收websocket连接
                var websocket = await context.WebSockets.AcceptWebSocketAsync();
                var clientId = Guid.NewGuid().ToString();
                var wsClient = new WebsocketClient
                {
                    Id = clientId,
                    WebSocket = websocket
                };
                try
                {
                    await HandleAsync(wsClient);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,$"Echo websocket client {0} err .",clientId);
                    throw;
                }
            }
            else
            {
                context.Response.StatusCode = HttpStatusCode.NotFound.GetHashCode();
            }
        }
        else
        {
            await _next(context);
        }
    }

    /// <summary>
    ///  处理客户端发送的消息
    /// </summary>
    /// <param name="wsClient"></param>
    /// <returns></returns>
    public async Task HandleAsync(WebsocketClient wsClient)
    {
        WebsocketClientCollection.Add(wsClient);
        _logger.LogInformation($"Websocket client 已添加.");

        WebSocketReceiveResult result = null;
        do
        {
            var buffer = new byte[1024 * 1];
            result = await wsClient.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
            {
                var msgString = Encoding.UTF8.GetString(buffer);
                _logger.LogInformation($"Websocket client ReceiveAsync message {msgString}.");
                var message = JsonConvert.DeserializeObject<Message>(msgString)!;
                message.SendClientId = wsClient.Id;
                wsClient.ReceiveResult = result;
                await MessageRoute(message);
            }
        }
        while (!result.CloseStatus.HasValue);
        WebsocketClientCollection.Remove(wsClient);
        _logger.LogInformation($"Websocket client 已关闭.");
    }

    /// <summary>
    /// 向客户端转发消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task MessageRoute(Message message)
    {
        var client = WebsocketClientCollection.Get(message.SendClientId);
        if (client != null)
        {
            switch (message.Action)
            {
                case "join":
                    client.RoomNo = message.Msg;
                    await client.SendMessageAsync($"{message.Nick} join room {client.RoomNo} success .");
                    _logger.LogInformation($"Websocket client {message.SendClientId} join room {client.RoomNo}.");
                    break;
                case "send_to_room":
                    if (string.IsNullOrEmpty(client.RoomNo))
                    {
                        break;
                    }
                    client.RoomNo = message.Msg;
                    var clients = WebsocketClientCollection.GetClientsByRoomNo(client.RoomNo);
                    clients.ForEach(async c => await c.SendMessageAsync(message.Nick + " : " + message.Msg));
                    _logger.LogInformation($"Websocket client {message.SendClientId} send message {message.Msg} to room {client.RoomNo}");
                    break;
                case "leave":

                    #region [通过把连接的RoomNo置空模拟关闭连接]
                    
                    var roomNo = client.RoomNo;
                    client.RoomNo = "";

                    #endregion

                    #region [后台关闭连接]

                    await client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closeddd", CancellationToken.None);
                    WebsocketClientCollection.Remove(client);

                    #endregion
                    await client.SendMessageAsync($"{message.Nick} leave room {roomNo} success .");
                    _logger.LogInformation($"Websocket client {message.SendClientId} leave room {roomNo}");
                    break;
                default:
                    break;
            }
        }
        else
        {
            _logger.LogInformation($"Websocket client id：{0} 未连接.", message.SendClientId);
        }
    }
}

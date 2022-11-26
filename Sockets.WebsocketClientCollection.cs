namespace WebSocketDemo.Sockets;

public class WebsocketClientCollection
{

    private static List<WebsocketClient> _clients = new List<WebsocketClient>();

    public static void Add(WebsocketClient client)
    {
        _clients.Add(client);
    }

    public static void Remove(WebsocketClient client)
    {
        _clients.Remove(client);
    }

    public static WebsocketClient? Get(string clientId)
    {
        var client = _clients.FirstOrDefault(m => m.Id == clientId);
        return client;
    }
    public static List<WebsocketClient> GetAll()
    {
        return _clients;
    }

    public static List<WebsocketClient> GetClientsByRoomNo(string roomNo)
    {
        return _clients.Where(m => m.RoomNo == roomNo).ToList();
    }
}

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();

        using var client = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:6969/ws?name={name}");
        await client.ConnectAsync(uri, CancellationToken.None);
        Console.WriteLine("Connected to the server");

        var receiveTask = ReceiveMessages(client);
        var sendTask = SendMessages(client, name);

        await Task.WhenAll(receiveTask, sendTask);
    }

    static async Task SendMessages(ClientWebSocket client, string name)
    {
        while (client.State == WebSocketState.Open)
        {
            string message = Console.ReadLine();
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes);
            await client.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    static async Task ReceiveMessages(ClientWebSocket client)
    {
        var buffer = new byte[1024 * 4];
        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            }
            else
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine(message);
            }
        }
    }
}

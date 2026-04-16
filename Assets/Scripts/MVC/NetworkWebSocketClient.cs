using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetworkWebSocketClient
{
    private static ClientWebSocket client;
    private static CancellationTokenSource cancellationTokenSource;

    public static bool IsConnected => client != null && client.State == WebSocketState.Open;

    public static async Task Connect(string url = "ws://127.0.0.1:5000")
    {
        if (IsConnected)
        {
            return;
        }

        try
        {
            client = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            await client.ConnectAsync(new Uri(url), cancellationTokenSource.Token);

            NetworkManager.OnSend -= HandleSend;
            NetworkManager.OnSend += HandleSend;

            _ = ReceiveMessage();
        }
        catch (Exception exception)
        {
            Debug.LogError($"WebSocket connect failed: {exception.Message}");
            await Disconnect();
        }
    }

    public static async Task ReceiveMessage()
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (IsConnected && !cancellationTokenSource.IsCancellationRequested)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await client.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            cancellationTokenSource.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await Disconnect();
                            return;
                        }

                        memoryStream.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    string json = Encoding.UTF8.GetString(memoryStream.ToArray());
                    WebSocketPacket packet = JsonUtility.FromJson<WebSocketPacket>(json);
                    if (packet == null || string.IsNullOrEmpty(packet.protocolName))
                    {
                        continue;
                    }

                    byte[] payload = string.IsNullOrEmpty(packet.payload)
                        ? Array.Empty<byte>()
                        : Convert.FromBase64String(packet.payload);

                    NetworkManager.Notify(packet.protocolName, payload);
                }
            }
        }
        catch (Exception exception)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                Debug.LogError($"WebSocket receive failed: {exception.Message}");
            }
        }
    }

    public static async Task Disconnect()
    {
        NetworkManager.OnSend -= HandleSend;

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }

        if (client != null)
        {
            try
            {
                if (client.State == WebSocketState.Open || client.State == WebSocketState.CloseReceived)
                {
                    await client.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "disconnect",
                        CancellationToken.None);
                }
            }
            catch
            {
            }

            client.Dispose();
            client = null;
        }

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

    private static void HandleSend(string protocolName, byte[] payload)
    {
        _ = SendMessage(protocolName, payload);
    }

    private static async Task SendMessage(string protocolName, byte[] payload)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("WebSocket is not connected.");
            return;
        }

        try
        {
            WebSocketPacket packet = new WebSocketPacket
            {
                protocolName = protocolName,
                payload = Convert.ToBase64String(payload ?? Array.Empty<byte>())
            };

            string json = JsonUtility.ToJson(packet);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await client.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationTokenSource.Token);
        }
        catch (Exception exception)
        {
            Debug.LogError($"WebSocket send failed: {exception.Message}");
        }
    }

    [Serializable]
    private class WebSocketPacket
    {
        public string protocolName;
        public string payload;
    }
}

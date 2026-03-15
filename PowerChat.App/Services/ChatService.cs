using Microsoft.AspNetCore.SignalR.Client;
using PowerChat.App.Models;
using System.Threading.Channels;

namespace PowerChat.App.Services
{
    public class ChatService
    {
        private readonly DatabaseService _database;
        private readonly HubConnection _hubConnection;
        private const string ServerUrl = "https://powerproject.onrender.com/chat";

        public event Action<ChatMessage> OnMessageReceived = delegate { };
        private readonly Channel<ChatMessage> _messageQueue = Channel.CreateUnbounded<ChatMessage>();

        public ChatService(DatabaseService database)
        {
            _database = database;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ServerUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ChatMessage>("ReceiveMessage", async (message) =>
            {
                message.Status = "\uf00c";
                bool inserted = await _database.SaveMessageAsync(message);
                if (!inserted)
                {
                    return;
                }

                OnMessageReceived.Invoke(message);
            });

            _ = Task.Run(() => ProcessQueueAsync(CancellationToken.None));
        }

        public async Task Connect()
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connect Error: {ex.Message}");
            }
        }

        public string GetConnectionStatus()
        {
            return _hubConnection.State.ToString() ?? "Unknown";
        }

        public async Task<bool> SendPayload(ChatMessage message)
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                return false;
            }

            try
            {
                await _hubConnection.InvokeAsync("SendPayload", message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendPayload: {ex.Message}");
                return false;
            }
        }

        public async Task EnqueueMessage(ChatMessage message)
        {
            await _database.SaveMessageAsync(message);
            _messageQueue.Writer.TryWrite(message);
        }

        public async Task ProcessQueueAsync(CancellationToken ct)
        {
            await foreach (var message in _messageQueue.Reader.ReadAllAsync(ct))
            {
                bool sent = false;
                DateTime startTime = DateTime.Now;

                while (!sent && (DateTime.Now - startTime).TotalSeconds < 60)
                {
                    sent = await SendPayload(message);

                    if (sent)
                    {
                        break;
                    }
                    await Task.Delay(2500, ct);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    message.Status = sent ? "\uf00c" : "\uf00d";
                });

                await _database.UpdateMessageAsync(message);
                await Task.Delay(250, ct);
            }
        }
    }
}
using Microsoft.AspNetCore.SignalR.Client;
using PowerChat.App.Models;
using System.Threading.Channels;

namespace PowerChat.App.Services
{
    public class ChatService
    {
        private readonly HubConnection _hubConnection;
        private const string ServerUrl = "https://powerproject.onrender.com/chat";

        public event Action<string, string> OnMessageReceived = delegate { };
        private readonly Channel<ChatMessage> _messageQueue = Channel.CreateUnbounded<ChatMessage>();

        public ChatService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ServerUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, payload) =>
            {
                OnMessageReceived.Invoke(user, payload);
            });
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

        public async Task<bool> SendPayload(string user, string payload)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendPayload", user, payload);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendPayload: {ex.Message}");
                return false;
            }
        }

        public string GetConnectionStatus() => _hubConnection.State.ToString() ?? "Unknown";

        public void EnqueueMessage(ChatMessage message)
        {
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
                    sent = await SendPayload(message.User, message.Text);

                    if (sent)
                    {
                        break;
                    }
                    await Task.Delay(2500, ct);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    message.Status = sent ? "\uf00c" : "\uf00d";
                });

                await Task.Delay(250, ct);
            }
        }
    }
}
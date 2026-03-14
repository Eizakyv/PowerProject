using PowerChat.App.Models;
using PowerChat.App.Services;
using System.Collections.ObjectModel;

namespace PowerChat.App
{
    public partial class MainPage : ContentPage
    {
        private readonly ChatService _chatService;
        public ObservableCollection<ChatMessage> Messages
        {
            get;
        }
        = [];

        public MainPage(ChatService chatService)
        {
            InitializeComponent();

            MessagesListView.ItemsSource = Messages;
            _chatService = chatService;

            ConnectToServer();
        }

        private void ChatService_OnMessageReceived(string user, string message)
        {
            var newMessage = new ChatMessage
            {
                User = user,
                Text = message,
                Timestamp = DateTime.Now,
                Status = "\uf00c"
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(newMessage);
                MessagesListView.ScrollTo(newMessage, ScrollToPosition.End, animate: false);
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _chatService.OnMessageReceived -= ChatService_OnMessageReceived;
            _chatService.OnMessageReceived += ChatService_OnMessageReceived;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _chatService.OnMessageReceived -= ChatService_OnMessageReceived;
        }

        private async void ConnectToServer()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _chatService.Connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connect Error: {ex.Message}");
                }
            });

            _ = Task.Run
            (
                () => _chatService.ProcessQueueAsync(CancellationToken.None)
            );
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageEntry.Text))
            {
                return;
            }

            string payload = MessageEntry.Text;
            MessageEntry.Text = string.Empty;
            MessageEntry.Focus();

            var newMessage = new ChatMessage
            {
                User = "Me",
                Text = payload,
                Timestamp = DateTime.Now,
                Status = "\uf017"
            };

            Messages.Add(newMessage);
            MessagesListView.ScrollTo(newMessage, ScrollToPosition.End, animate: false);

            // Enqueue the message to be sent to the server
            _chatService.EnqueueMessage(newMessage);
        }
    }
}
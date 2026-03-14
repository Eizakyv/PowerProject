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
            _chatService = chatService;

            MessagesListView.ItemsSource = Messages;
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            var newMessage = new ChatMessage
            {
                User = "Log",
                Text = "Conection status: " + _chatService.GetConnectionStatus(),
                Timestamp = DateTime.Now,
                Status = "\uf017"
            };
            Messages.Add(newMessage);
            MessagesListView.ScrollTo(newMessage, ScrollToPosition.End, animate: false);

            Task.Run(async () =>
            {
                await _chatService.Connect();
                Dispatcher.Dispatch(() =>
                {
                    newMessage.Text = "Conection status: " + _chatService.GetConnectionStatus();
                    newMessage.Status = "\uf00c";
                });
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

        private void ChatService_OnMessageReceived(string user, string payload)
        {
            var newMessage = new ChatMessage
            {
                User = user,
                Text = payload,
                Timestamp = DateTime.Now,
                Status = "\uf00c"
            };

            Dispatcher.Dispatch(() =>
            {
                Messages.Add(newMessage);
                MessagesListView.ScrollTo(newMessage, ScrollToPosition.End, animate: false);
            });
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageEntry.Text))
            {
                return;
            }

            string payload = MessageEntry.Text;
            MessageEntry.Text = string.Empty;

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
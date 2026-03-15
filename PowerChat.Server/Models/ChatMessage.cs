namespace PowerChat.Server.Models
{
    public class ChatMessage
    {
        public Guid MessageId
        {
            get;
            set;
        }

        public int Id { get; set; }

        public string User { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
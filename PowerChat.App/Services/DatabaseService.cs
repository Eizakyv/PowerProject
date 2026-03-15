using SQLite;
using PowerChat.App.Models;

namespace PowerChat.App.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _isInitialized;

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        private async Task InitializeAsync()
        {
            if (!_isInitialized)
            {
                await _database.CreateTableAsync<ChatMessage>();
                _isInitialized = true;
            }
        }

        public async Task<ChatMessage[]> GetMessages()
        {
            await InitializeAsync();
            var list = await _database.Table<ChatMessage>()
                                      .OrderBy(m => m.Timestamp)
                                      .ToListAsync();
            return [.. list];
        }

        public async Task<bool> SaveMessageAsync(ChatMessage message)
        {
            await InitializeAsync();

            try
            {
                await _database.InsertAsync(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            await InitializeAsync();
            await _database.UpdateAsync(message);
        }
    }
}
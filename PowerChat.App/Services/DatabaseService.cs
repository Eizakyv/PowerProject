using SQLite;
using PowerChat.App.Models;

namespace PowerChat.App.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _isInitialized;

        private readonly SemaphoreSlim _semaphore = new(1, 1); // to ensure thread-safe initialization

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            _database = new SQLiteAsyncConnection(dbPath);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await _semaphore.WaitAsync(); // acquire the semaphore to ensure only one thread initializes the database
            try
            {
                if (!_isInitialized) // double-check after acquiring the semaphore
                {
                    await _database.CreateTableAsync<ChatMessage>();
                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing the database: {ex.Message}");
            }
            finally
            {
                _semaphore.Release(); // release the semaphore regardless of success or failure
            }
        }

        public async Task<ChatMessage[]> GetMessages()
        {
            await InitializeAsync();

            try
            {
                var list = await _database.Table<ChatMessage>()
                                          .OrderBy(m => m.Timestamp)
                                          .ToListAsync();
                return [.. list];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching messages: {ex.Message}");
                return [];
            }
        }

        public async Task<bool> SaveMessageAsync(ChatMessage message)
        {
            await InitializeAsync();

            try
            {
                await _database.InsertAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving messages in the phone's database: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateMessageAsync(ChatMessage message)
        {
            await InitializeAsync();

            try
            {
                await _database.UpdateAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating a message in the phone's database: {ex.Message}");
                return false;
            }
        }
    }
}
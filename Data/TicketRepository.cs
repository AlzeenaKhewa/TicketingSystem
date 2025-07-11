// TicketingSystem/Data/TicketRepository.cs
using Dapper;
using Npgsql;
using TicketingSystem.Models;

namespace TicketingSystem.Data
{
    public class TicketRepository : ITicketRepository
    {
        private readonly string _connectionString;

        public TicketRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(int customerId)
        {
            // UPDATE: Added 'customer_name' to the SELECT statement to populate the model property.
            var sql = @"
                SELECT
                    id AS Id,
                    customer_id AS CustomerId,
                    customer_name AS CustomerName, -- <-- SELECT THE NEW COLUMN
                    title AS Title,
                    topic AS Topic,
                    description AS Description,
                    status AS Status,
                    priority AS Priority,
                    created_at AS CreatedAt,
                    account_number as AccountNumber
                FROM tickets
                WHERE customer_id = @CustomerId
                ORDER BY created_at DESC";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Ticket>(sql, new { CustomerId = customerId });
            }
        }

        public async Task<int> CreateTicketAsync(Ticket ticket)
        {
            // This implementation is already correct as per your previous request.
            var sql = @"
INSERT INTO tickets (customer_id, customer_name, title, topic, description, status, priority, created_at, account_number)
VALUES (@CustomerId, @CustomerName, @Title, @Topic, @Description, @Status, @Priority, @CreatedAt, @AccountNumber)
RETURNING id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var newId = await connection.ExecuteScalarAsync<int>(sql, ticket);
                return newId;
            }
        }

        // --- NEW METHODS FOR CHAT FUNCTIONALITY ---

        /// <summary>
        /// Retrieves all chat messages for a specific ticket, ordered chronologically.
        /// </summary>
        /// <param name="ticketId">The ID of the ticket.</param>
        /// <returns>A collection of ChatMessage objects.</returns>
        public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(int ticketId)
        {
            // Dapper will automatically map the snake_case columns from the DB 
            // to the PascalCase properties in the ChatMessage model.
            var sql = @"
                SELECT 
                    id AS Id,
                    ticket_id as TicketId,
                    sender_name as SenderName,
                    message as Message,
                    is_admin_message as IsAdminMessage,
                    sent_at as SentAt
                FROM chat_messages 
                WHERE ticket_id = @TicketId 
                ORDER BY sent_at ASC";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryAsync<ChatMessage>(sql, new { TicketId = ticketId });
            }
        }

        /// <summary>
        /// Inserts a new chat message into the database.
        /// </summary>
        /// <param name="message">The ChatMessage object to save.</param>
        public async Task CreateChatMessageAsync(ChatMessage message)
        {
            var sql = @"
                INSERT INTO chat_messages (ticket_id, sender_name, message, is_admin_message, sent_at)
                VALUES (@TicketId, @SenderName, @Message, @IsAdminMessage, @SentAt)";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, message);
            }
        }
    }
}
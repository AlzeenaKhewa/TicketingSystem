using TicketingSystem.Models;
using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TicketingSystem.Data
{
    public class ChatRepository : IChatRepository
    {
        private readonly string _connectionString;

        public ChatRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<ChatUserViewModel>> GetUsersForTicketAsync(int ticketId)
        {
            // --- THIS QUERY IS NOW CORRECT AND PREVENTS DUPLICATES ---
            // It fetches the customer for the specific ticket, plus all support/admin staff,
            // and uses an EXISTS subquery to check for online status without creating duplicate rows.
            const string sql = @"
                WITH TicketCustomer AS (
                    SELECT ""CustomerId"" FROM ""Tickets"" WHERE ""Id"" = @TicketId
                )
                SELECT 
                    u.""Id"" AS UserId, 
                    u.""Name"", 
                    u.""Role"",
                    EXISTS (SELECT 1 FROM ""Connections"" c WHERE c.""UserId"" = u.""Id"") AS IsOnline
                FROM ""Users"" u
                WHERE u.""Role"" IN ('Support', 'Admin') OR u.""Id"" = (SELECT ""CustomerId"" FROM TicketCustomer)
                ORDER BY u.""Role"", u.""Name"";";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ChatUserViewModel>(sql, new { TicketId = ticketId });
        }

        // --- ALL OTHER METHODS IN THIS FILE ARE CORRECT AND REMAIN UNCHANGED ---

        public async Task<IEnumerable<ChatMessage>> GetConversationAsync(int ticketId, int currentUserId)
        {
            const string sql = @"
                SELECT 
                    cm.""Id"", cm.""Content"", cm.""Timestamp"", cm.""TicketId"", cm.""SenderId"",
                    sender.""Name"" AS SenderName, sender.""Role"" AS SenderRole,
                    cm.""RecipientId"", recipient.""Name"" AS RecipientName
                FROM ""ChatMessages"" cm
                JOIN ""Users"" sender ON cm.""SenderId"" = sender.""Id""
                LEFT JOIN ""Users"" recipient ON cm.""RecipientId"" = recipient.""Id""
                WHERE cm.""TicketId"" = @TicketId
                  AND (cm.""RecipientId"" IS NULL OR cm.""SenderId"" = @CurrentUserId OR cm.""RecipientId"" = @CurrentUserId)
                ORDER BY cm.""Timestamp"";
            ";
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ChatMessage>(sql, new { TicketId = ticketId, CurrentUserId = currentUserId });
        }

        public async Task<ChatMessage?> SaveMessageAsync(ChatMessage message)
        {
            const string sql = @"
                INSERT INTO ""ChatMessages"" (""Content"", ""TicketId"", ""SenderId"", ""RecipientId"", ""Timestamp"")
                VALUES (@Content, @TicketId, @SenderId, @RecipientId, @Timestamp)
                RETURNING ""Id"";";
            using var connection = new NpgsqlConnection(_connectionString);
            var id = await connection.ExecuteScalarAsync<int>(sql, message);

            const string getSql = @"
                 SELECT cm.""Id"", cm.""Content"", cm.""Timestamp"", cm.""TicketId"", cm.""SenderId"",
                    sender.""Name"" AS SenderName, sender.""Role"" AS SenderRole,
                    cm.""RecipientId"", recipient.""Name"" AS RecipientName
                FROM ""ChatMessages"" cm
                JOIN ""Users"" sender ON cm.""SenderId"" = sender.""Id""
                LEFT JOIN ""Users"" recipient ON cm.""RecipientId"" = recipient.""Id""
                WHERE cm.""Id"" = @Id;";
            return await connection.QuerySingleOrDefaultAsync<ChatMessage>(getSql, new { Id = id });
        }

        public async Task AddOrUpdateConnectionAsync(string connectionId, int userId)
        {
            const string sql = @"
                INSERT INTO ""Connections"" (""ConnectionId"", ""UserId"", ""ConnectedAt"")
                VALUES (@ConnectionId, @UserId, @ConnectedAt)
                ON CONFLICT (""ConnectionId"")
                DO UPDATE SET ""UserId"" = @UserId, ""ConnectedAt"" = @ConnectedAt;";
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { ConnectionId = connectionId, UserId = userId, ConnectedAt = DateTime.UtcNow });
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            const string sql = @"DELETE FROM ""Connections"" WHERE ""ConnectionId"" = @ConnectionId;";
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { ConnectionId = connectionId });
        }

        public async Task<IEnumerable<string>> GetUserConnectionsAsync(int userId)
        {
            const string sql = @"SELECT ""ConnectionId"" FROM ""Connections"" WHERE ""UserId"" = @UserId;";
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<string>(sql, new { UserId = userId });
        }
    }
}
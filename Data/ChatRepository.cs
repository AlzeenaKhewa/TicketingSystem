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

        public async Task<IEnumerable<ChatUserViewModel>> GetUsersForTicketAsync(int ticketId)
        {
            const string sql = @"
                SELECT 
                    u.""Id"" AS UserId, u.""Name"", u.""Role"",
                    CASE WHEN c.""ConnectionId"" IS NOT NULL THEN TRUE ELSE FALSE END AS IsOnline
                FROM ""Users"" u
                LEFT JOIN ( SELECT DISTINCT ""UserId"", ""ConnectionId"" FROM ""Connections"" ) c 
                    ON u.""Id"" = c.""UserId""
                ORDER BY u.""Name"";";
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ChatUserViewModel>(sql);
        }
    }
}
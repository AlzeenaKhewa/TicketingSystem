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

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            // FIX: Replaced SELECT * with an explicit, quoted column list.
            const string sql = @"
                SELECT 
                    ""Id"", ""CustomerId"", ""CustomerName"", ""Title"", ""Topic"", ""Description"", 
                    ""Status"", ""Priority"", ""CreatedAt"", ""AccountNumber""
                FROM ""Tickets"" 
                ORDER BY ""CreatedAt"" DESC";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Ticket>(sql);
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(int customerId)
        {
            // FIX: Replaced SELECT * with an explicit, quoted column list.
            const string sql = @"
                SELECT 
                    ""Id"", ""CustomerId"", ""CustomerName"", ""Title"", ""Topic"", ""Description"", 
                    ""Status"", ""Priority"", ""CreatedAt"", ""AccountNumber""
                FROM ""Tickets"" 
                WHERE ""CustomerId"" = @CustomerId 
                ORDER BY ""CreatedAt"" DESC";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Ticket>(sql, new { CustomerId = customerId });
            }
        }

        public async Task<int> CreateTicketAsync(Ticket ticket)
        {
            // This method was already correct, but included for completeness.
            const string sql = @"
                INSERT INTO ""Tickets"" (""CustomerId"", ""CustomerName"", ""Title"", ""Topic"", ""Description"", ""Status"", ""Priority"", ""CreatedAt"", ""AccountNumber"")
                VALUES (@CustomerId, @CustomerName, @Title, @Topic, @Description, @Status, @Priority, @CreatedAt, @AccountNumber)
                RETURNING ""Id"";";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var newId = await connection.ExecuteScalarAsync<int>(sql, ticket);
                return newId;
            }
        }

        public async Task<Ticket?> GetByIdAsync(int ticketId)
        {
            // FIX: Replaced SELECT * with an explicit, quoted column list.
            const string sql = @"
                SELECT 
                    ""Id"", ""CustomerId"", ""CustomerName"", ""Title"", ""Topic"", ""Description"", 
                    ""Status"", ""Priority"", ""CreatedAt"", ""AccountNumber""
                FROM ""Tickets"" 
                WHERE ""Id"" = @TicketId";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Ticket>(sql, new { TicketId = ticketId });
        }
    }
}
using System.Security.Cryptography;
using System.Text;

namespace TicketingSystem.Helpers
{
    public static class AvatarHelper
    {
        private static readonly string[] _colors = new[]
        {
            "#1abc9c", "#2ecc71", "#3498db", "#9b59b6", "#34495e",
            "#16a085", "#27ae60", "#2980b9", "#8e44ad", "#2c3e50",
            "#f1c40f", "#e67e22", "#e74c3c", "#ecf0f1", "#95a5a6",
            "#f39c12", "#d35400", "#c0392b", "#bdc3c7", "#7f8c8d"
        };

        /// <summary>
        /// Gets the user's initials from their full name.
        /// </summary>
        public static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            }
            return name.Length > 1 ? name.Substring(0, 2).ToUpper() : name.ToUpper();
        }

        /// <summary>
        /// Generates a consistent background color based on the user's name.
        /// </summary>
        public static string GetBackgroundColor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return _colors[0];

            // Use a simple hash to get a consistent color for a user
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(name));
            int index = hash[0] % _colors.Length;
            return _colors[index];
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string UserMessage { get; set; } = string.Empty;

        [Required]
        public string BotResponse { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
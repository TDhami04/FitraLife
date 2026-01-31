using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitraLife.Models
{
    public class ChatSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public string Title { get; set; } = "New Chat";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ChatMessage> Messages { get; set; } = new();
    }
}

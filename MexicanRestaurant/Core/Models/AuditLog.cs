namespace MexicanRestaurant.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ActionType { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

namespace TableOrder_Hust.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // có thể null nếu khách không đăng ký tài khoản
        public int TableId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty; // 18:30
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, CheckedIn, NoShow

        public User? User { get; set; }
        public Table? Table { get; set; }
    }
}

namespace TableOrder_Hust.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int TableId { get; set; }
        public Table Table { get; set; }

        public DateTime ReservationTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, NoShow
    }
}

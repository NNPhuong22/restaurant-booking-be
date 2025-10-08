namespace TableOrder_Hust.DTOs
{
    public class CreateReservationRequest
    {
        public int? UserId { get; set; } // null nếu khách vãng lai
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
    }

    public class ReservationResponse
    {
        public int Id { get; set; }
        public DateTime ReservationTime { get; set; }
        public int TableId { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = "";
    }
}

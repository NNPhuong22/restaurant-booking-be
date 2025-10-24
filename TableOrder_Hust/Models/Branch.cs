namespace TableOrder_Hust.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public ICollection<Table>? Tables { get; set; }
    }
}

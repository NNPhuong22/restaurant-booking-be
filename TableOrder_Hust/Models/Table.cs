namespace TableOrder_Hust.Models
{
    public class Table
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Branch? Branch { get; set; }
    }
}

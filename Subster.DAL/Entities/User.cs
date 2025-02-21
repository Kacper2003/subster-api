namespace Subster.DAL.Entities
{
    // Base user í bili, líklegast splittað í einkaþjálfara og notanda
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        // Kennitala
        public string Ssn { get; set; } = null!;
        // Símanúmer, bæði ætti að koma frá Taktikal
        public string PhoneNumber { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
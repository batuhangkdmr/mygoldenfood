namespace MyGoldenFood.Models
{
    public class Benefit : BaseEntity
    {
        public string Name { get; set; } // Ürün adı
        public string Content { get; set; } // Fayda açıklaması
        public string? ImagePath { get; set; } // Resim yolu
    }
}

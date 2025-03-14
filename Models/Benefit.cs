using System.ComponentModel.DataAnnotations.Schema;

namespace MyGoldenFood.Models
{
    public class Benefit : BaseEntity
    {
        public string Name { get; set; } // Ürün adı
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; } // Fayda açıklaması
        public string? ImagePath { get; set; } // Resim yolu
                                               // 📌 **Çeviri İlişkisi**
        public virtual ICollection<BenefitTranslation> Translations { get; set; } = new List<BenefitTranslation>();
    }
}

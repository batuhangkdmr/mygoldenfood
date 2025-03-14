using System.ComponentModel.DataAnnotations.Schema;

namespace MyGoldenFood.Models
{
    public class ProductTranslation : BaseEntity
    {
        public int ProductId { get; set; } // Foreign Key
        public string LanguageCode { get; set; } // "en", "fr", "de" gibi ISO kodu
        public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? Description { get; set; }

        public virtual Product Product { get; set; } // Bağlantılı ana ürün
    }
}

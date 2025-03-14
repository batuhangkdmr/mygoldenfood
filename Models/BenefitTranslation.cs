using System.ComponentModel.DataAnnotations.Schema;

namespace MyGoldenFood.Models
{
    public class BenefitTranslation : BaseEntity
    {
        public int BenefitId { get; set; } // Foreign Key
        public string LanguageCode { get; set; } // "en", "fr", "de"
        public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; }

        public virtual Benefit Benefit { get; set; } // Bağlantılı fayda
    }
}

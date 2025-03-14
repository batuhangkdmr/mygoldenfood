using System.ComponentModel.DataAnnotations.Schema;

namespace MyGoldenFood.Models
{
    public class RecipeTranslation : BaseEntity
    {
        public int RecipeId { get; set; } // Foreign Key
        public string LanguageCode { get; set; } // "en", "fr", "de"
        public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; }

        public virtual Recipe Recipe { get; set; } // Bağlantılı tarif
    }
}

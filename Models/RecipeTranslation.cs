namespace MyGoldenFood.Models
{
    public class RecipeTranslation : BaseEntity
    {
        public int RecipeId { get; set; } // Foreign Key
        public string LanguageCode { get; set; } // "en", "fr", "de"
        public string Name { get; set; }
        public string Content { get; set; }

        public virtual Recipe Recipe { get; set; } // Bağlantılı tarif
    }
}

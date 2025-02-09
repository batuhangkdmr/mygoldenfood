namespace MyGoldenFood.Models
{
    public class RecipeCategoryTranslation : BaseEntity
    {
        public int RecipeCategoryId { get; set; } // Bağlı olduğu kategori
        public string Language { get; set; } // Dil kodu (en, de, fr, ru, ja, ko)
        public string Name { get; set; } // Çevrilen kategori adı

        public virtual RecipeCategory RecipeCategory { get; set; } // İlişki
    }
}

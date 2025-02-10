namespace MyGoldenFood.Models
{
    public class Recipe : BaseEntity
    {
        public string Name { get; set; } // Tarif adı
        public string Content { get; set; } // Tarif içeriği
        public string? ImagePath { get; set; } // Resim yolu
        public int? RecipeCategoryId { get; set; } // Kategori ID'si
        public virtual RecipeCategory? RecipeCategory { get; set; } // Kategori ile ilişki
        // ✅ ÇEVİRİLERLE İLİŞKİ EKLENDİ
        public virtual ICollection<RecipeTranslation> RecipeTranslations { get; set; } = new List<RecipeTranslation>();
    }
}

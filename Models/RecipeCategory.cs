namespace MyGoldenFood.Models
{
    public class RecipeCategory : BaseEntity
    {
        public string Name { get; set; } // Kategori adı
        public string ImagePath { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; } // Tarifler ile ilişki
    }
}

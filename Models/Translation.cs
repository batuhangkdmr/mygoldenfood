namespace MyGoldenFood.Models
{
    public class Translation : BaseEntity
    {
        public string EntityType { get; set; } // Product, Recipe, Benefit gibi
        public int EntityId { get; set; } // Çeviriye ait ID
        public string FieldName { get; set; } // Örneğin "Name" veya "Description"
        public string Language { get; set; } // Hedef dil (TR, EN, DE vb.)
        public string TranslatedText { get; set; } // Çevrilen metin
    }
}

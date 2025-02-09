namespace MyGoldenFood.Models
{
    public class Translation : BaseEntity
    {
        public int ReferenceId { get; set; } // Ürün/Tarif/Fayda ID'si
        public string TableName { get; set; } // "Product", "Recipe", "Benefit"
        public string FieldName { get; set; }
        public string Language { get; set; } // tr, en, de, fr, ru, ja, ko
        public string TranslatedValue { get; set; } // Çevrilen değer
    }
}

namespace MyGoldenFood.Models
{
    public class BenefitTranslation : BaseEntity
    {
        public int BenefitId { get; set; } // Foreign Key
        public string LanguageCode { get; set; } // "en", "fr", "de"
        public string Name { get; set; }
        public string Content { get; set; }

        public virtual Benefit Benefit { get; set; } // Bağlantılı fayda
    }
}

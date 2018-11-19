using Gcpe.Hub.API.Helpers;

namespace Gcpe.Hub.API.Helpers
{
    public partial class SectorLanguage
    {
        public System.Guid SectorId { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; }

        public virtual Language Language { get; set; }
        public virtual Sector Sector { get; set; }
    }
}

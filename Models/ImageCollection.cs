using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace image_upload.Models
{
    public class ImageCollection
    {
        public int UserID { get; set; }
        public string PhotoPath { get; set; } = string.Empty;
    }

    public class ImageCollectionConfiguration : IEntityTypeConfiguration<ImageCollection>
    {
        public void Configure(EntityTypeBuilder<ImageCollection> builder)
        {
            builder.HasKey(e => e.UserID);
        }
    }
}

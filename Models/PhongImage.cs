using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyPhongTro.Models
{
    public class PhongImage
    {
        [Key]
        public int MaImage { get; set; }

        [Required]
        public int MaPhong { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

        public DateTime NgayUpload { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Phong? Phong { get; set; }
    }
}
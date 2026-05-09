using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class BaiDang
    {
        [Key]
        public int MaBaiDang { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255, ErrorMessage = "Tiêu đề không quá 255 ký tự")]
        public string? TieuDe { get; set; }

        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }

        public DateTime? NgayDang { get; set; } = DateTime.Now;

        public string? TrangThai { get; set; } = "Hiển thị";

        [ForeignKey("MaPhong")]
        public virtual Phong? PhongNavigation { get; set; }
    }
}
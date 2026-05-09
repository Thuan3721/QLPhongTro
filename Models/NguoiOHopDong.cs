using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class NguoiOHopDong
    {
        [Key]
        public int MaNguoiO { get; set; }

        public int MaKhachHang { get; set; }  // 🔑 Liên kết với khách hàng

        public int? MaHopDong { get; set; }   // Liên kết với hợp đồng (sau khi tạo)

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; } = string.Empty;

        public string? CCCD { get; set; }

        public string? SoDienThoai { get; set; }

        public bool? LaNguoiDaiDien { get; set; } = false;

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHangNavigation { get; set; }

        [ForeignKey("MaHopDong")]
        public virtual HopDong? HopDongNavigation { get; set; }
    }
}
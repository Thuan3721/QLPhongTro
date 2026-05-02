using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class NguoiOHopDong
    {
        [Key]
        public int MaNguoiO { get; set; }

        public int MaHopDong { get; set; }

        public string? HoTen { get; set; }

        public string? CCCD { get; set; }

        public string? SoDienThoai { get; set; }

        public bool? LaNguoiDaiDien { get; set; } = false;

        // Thêm navigation property này
        [ForeignKey("MaHopDong")]
        public virtual HopDong? HopDongNavigation { get; set; }
    }
}
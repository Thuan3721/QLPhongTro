using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class HopDong
    {
        [Key]
        public int MaHopDong { get; set; }
        public int MaPhong { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public decimal? TienCoc { get; set; }
        public string? FileHopDong { get; set; }
        public string? TrangThai { get; set; }

        // Navigation properties
        [ForeignKey("MaPhong")]
        public virtual Phong? PhongNavigation { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHangNavigation { get; set; }
    }
}
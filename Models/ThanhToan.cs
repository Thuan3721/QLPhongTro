using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }
        public int MaHoaDon { get; set; }
        public decimal? SoTien { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string? NoiDungChuyenKhoan { get; set; }
        public string? TrangThai { get; set; }

        [ForeignKey("MaHoaDon")]
        public virtual HoaDon? HoaDonNavigation { get; set; }
    }
}
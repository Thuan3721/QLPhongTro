using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaHoaDon { get; set; }

        public string? LoaiKhoanThu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DonGia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ThanhTien { get; set; }

        public string? GhiChu { get; set; }

        // Navigation property - THÊM DÒNG NÀY
        [ForeignKey("MaHoaDon")]
        public virtual HoaDon? HoaDonNavigation { get; set; }
    }
}
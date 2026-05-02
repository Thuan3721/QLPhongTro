using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyPhongTro.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        public required string TenDangNhap { get; set; }

        public required string MatKhau { get; set; }

        public required string VaiTro { get; set; }

        public string? TrangThai { get; set; } = "Hoạt động";
    }
}
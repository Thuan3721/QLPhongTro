using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }
        public int MaCoSo { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public decimal GiaPhong { get; set; }
        public double? DienTich { get; set; }
        public string? TrangThai { get; set; }

        [ForeignKey("MaCoSo")]
        public virtual CoSo? CoSo { get; set; }
    }
}
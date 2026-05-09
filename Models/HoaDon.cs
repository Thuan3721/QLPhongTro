using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHoaDon { get; set; }
        public int MaHopDong { get; set; }
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }

        [ForeignKey("MaHopDong")]
        public virtual HopDong? HopDongNavigation { get; set; }
    }
}
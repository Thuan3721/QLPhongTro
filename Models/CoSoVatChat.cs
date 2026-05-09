using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class CoSoVatChat
    {
        [Key]
        public int MaCSVC { get; set; }

        public int MaPhong { get; set; }

        public string? TenThietBi { get; set; }

        public int? SoLuong { get; set; }

        public string? TinhTrang { get; set; }

        // Thêm Navigation Property này
        [ForeignKey("MaPhong")]
        public virtual Phong? PhongNavigation { get; set; }
    }
}
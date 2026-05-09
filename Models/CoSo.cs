using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyPhongTro.Models
{
    public class CoSo
    {
        [Key]
        public int MaCoSo { get; set; }

        public required string TenCoSo { get; set; }

        public string? DiaChi { get; set; }

        public string? MoTa { get; set; }
    }
}
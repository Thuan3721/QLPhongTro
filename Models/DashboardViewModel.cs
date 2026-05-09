namespace HeThongQuanLyPhongTro.Models
{
    public class DashboardViewModel
    {
        // Thống kê phòng
        public int TongSoPhong { get; set; }
        public int SoPhongDaThue { get; set; }
        public int SoPhongTrong { get; set; }
        public int TongSoKhachThue { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int SoHopDongSapHetHan { get; set; }
        public List<HopDongSapHetHan> HopDongSapHetHanList { get; set; } = new List<HopDongSapHetHan>();

        // THÊM THỐNG KÊ BÀI ĐĂNG
        public int TongSoBaiDang { get; set; }
        public int SoBaiDangHienThi { get; set; }
        public int SoBaiDangAn { get; set; }
        public int SoBaiDangThangNay { get; set; }
        public List<BaiDangGanDay> BaiDangGanDayList { get; set; } = new List<BaiDangGanDay>();
    }

    public class HopDongSapHetHan
    {
        public string TenPhong { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public DateTime NgayKetThuc { get; set; }
        public int SoNgayConLai { get; set; }
    }

    // THÊM CLASS MỚI
    public class BaiDangGanDay
    {
        public int MaBaiDang { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public DateTime NgayDang { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int LuotXem { get; set; }
    }
}
using HeThongQuanLyPhongTro.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text;

namespace HeThongQuanLyPhongTro.Services
{
    public class PdfHopDongService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfHopDongService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> XuatHopDongPdf(HopDong hopDong, KhachHang khachHang, Phong phong, CoSo coSo, string ngayKy, List<NguoiOHopDong> danhSachNguoiO)
        {
            // Đọc file HTML mẫu
            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "HopDongMau.html");
            string htmlContent = await File.ReadAllTextAsync(templatePath);

            // Tạo số hợp đồng dạng 1, 2, 3... (không có HD-)
            string soHopDong = hopDong.MaHopDong.ToString();

            // Thay thế các placeholder bằng dữ liệu thật
            htmlContent = htmlContent.Replace("{{SoHopDong}}", soHopDong);
            htmlContent = htmlContent.Replace("{{NgayKy}}", ngayKy);
            htmlContent = htmlContent.Replace("{{DiaChiCoSo}}", coSo?.DiaChi ?? "........................");
            htmlContent = htmlContent.Replace("{{HoTenKhach}}", khachHang?.HoTen ?? "........................");
            htmlContent = htmlContent.Replace("{{CCCDKhach}}", khachHang?.CCCD ?? "........................");
            htmlContent = htmlContent.Replace("{{SDTKhach}}", khachHang?.SoDienThoai ?? "........................");
            htmlContent = htmlContent.Replace("{{EmailKhach}}", khachHang?.Email ?? "........................");
            htmlContent = htmlContent.Replace("{{DiaChiKhach}}", khachHang?.DiaChi ?? "........................");
            htmlContent = htmlContent.Replace("{{TenPhong}}", phong?.TenPhong ?? "........................");
            htmlContent = htmlContent.Replace("{{DienTich}}", phong?.DienTich.ToString() ?? "0");
            htmlContent = htmlContent.Replace("{{GiaPhong}}", (phong?.GiaPhong ?? 0).ToString("N0"));
            htmlContent = htmlContent.Replace("{{NgayBatDau}}", hopDong.NgayBatDau?.ToString("dd/MM/yyyy") ?? "........................");
            htmlContent = htmlContent.Replace("{{NgayKetThuc}}", hopDong.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "........................");
            htmlContent = htmlContent.Replace("{{TienCoc}}", (hopDong.TienCoc ?? 0).ToString("N0"));

            // Thay thế danh sách người ở
            string bangNguoiO = TaoBangNguoiO(danhSachNguoiO);
            htmlContent = htmlContent.Replace("{{DanhSachNguoiO}}", bangNguoiO);

            // Chuyển HTML thành PDF
            await new BrowserFetcher().DownloadAsync();

            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(htmlContent);
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm" }
                });

                // Lưu file PDF
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "files", "hopdongs");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"HopDong_{hopDong.MaHopDong}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);

                return $"/files/hopdongs/{fileName}";
            }
        }

        private string TaoBangNguoiO(List<NguoiOHopDong> danhSach)
        {
            if (danhSach == null || !danhSach.Any())
            {
                return @"<tr><td colspan='5' style='text-align:center;'>Không có người ở chung</td></tr>";
            }

            StringBuilder html = new StringBuilder();
            int stt = 1;
            foreach (var nguoi in danhSach)
            {
                html.Append($@"
                <tr>
                    <td style='text-align:center;'>{stt}</td>
                    <td>{nguoi.HoTen ?? "...................."}</td>
                    <td>{nguoi.CCCD ?? "...................."}</td>
                    <td>{nguoi.SoDienThoai ?? "...................."}</td>
                    <td>{(nguoi.LaNguoiDaiDien == true ? "Đại diện" : "Thành viên")}</td>
                </tr>");
                stt++;
            }
            return html.ToString();
        }
    }
}
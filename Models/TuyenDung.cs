using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("TuyenDung")]
    public class TuyenDung
    {
        [Key]
        public int MaTuyenDung { get; set; }

        [Required(ErrorMessage = "Vị trí tuyển dụng không được để trống")]
        [StringLength(150)]
        [Display(Name = "Vị trí tuyển dụng")]
        public string ViTri { get; set; } = string.Empty;

        [Display(Name = "Phòng ban")]
        public int? MaPhongBan { get; set; }

        [Required(ErrorMessage = "Số lượng tuyển không được để trống")]
        [Range(1, 1000, ErrorMessage = "Số lượng tuyển phải lớn hơn 0")]
        [Display(Name = "Số lượng tuyển")]
        public int SoLuongCanTuyen { get; set; }

        [Display(Name = "Ngày đăng tin")]
        [DataType(DataType.Date)]
        public DateTime NgayDangTin { get; set; } = DateTime.Now;

        [Display(Name = "Hạn nộp hồ sơ")]
        [DataType(DataType.Date)]
        public DateTime? HanNopHoSo { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "DangTuyen";

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [ForeignKey(nameof(MaPhongBan))]
        public virtual PhongBan? PhongBan { get; set; }

        [NotMapped]
        [Display(Name = "Tên trạng thái")]
        public string TrangThaiText => TrangThai switch
        {
            "DungTuyen" => "Dừng tuyển",
            "DaTuyenDu" => "Đã tuyển đủ",
            _ => "Đang tuyển"
        };
    }
}

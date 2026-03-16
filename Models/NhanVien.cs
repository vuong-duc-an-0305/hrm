using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        [Display(Name = "Mã nhân viên")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        [Phone]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }

        [Display(Name = "Phòng ban")]
        public int? MaPhongBan { get; set; }

        [ForeignKey("MaPhongBan")]
        public virtual PhongBan? PhongBan { get; set; }

        // Navigation
        public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<NghiPhep> NghiPheps { get; set; } = new List<NghiPhep>();
    }
}

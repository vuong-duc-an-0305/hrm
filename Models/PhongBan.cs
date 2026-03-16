using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("PhongBan")]
    public class PhongBan
    {
        [Key]
        public int MaPhongBan { get; set; }

        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên phòng ban")]
        public string TenPhongBan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
        public virtual ICollection<TuyenDung> TuyenDungs { get; set; } = new List<TuyenDung>();
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    }
}

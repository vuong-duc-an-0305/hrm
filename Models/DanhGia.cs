using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        [Required(ErrorMessage = "Nhân viên không được để trống")]
        [Display(Name = "Nhân viên")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Kỳ đánh giá không được để trống")]
        [StringLength(50)]
        [Display(Name = "Kỳ đánh giá")]
        public string KyDanhGia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm đánh giá không được để trống")]
        [Range(0, 10, ErrorMessage = "Điểm đánh giá phải từ 0 đến 10")]
        [Display(Name = "Điểm đánh giá")]
        public double DiemDanhGia { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? NhanXet { get; set; }

        [Display(Name = "Ngày đánh giá")]
        [DataType(DataType.Date)]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}

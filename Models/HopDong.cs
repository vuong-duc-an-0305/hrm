using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("HopDong")]
    public class HopDong
    {
        [Key]
        public int MaHopDong { get; set; }

        [Required(ErrorMessage = "Nhân viên không được để trống")]
        [Display(Name = "Nhân viên")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Loại hợp đồng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Loại hợp đồng")]
        public string LoaiHopDong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Lương")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Luong { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }

        [NotMapped]
        [Display(Name = "Trạng thái")]
        public string TrangThai => NgayKetThuc.HasValue && NgayKetThuc.Value < DateTime.Now ? "Hết hạn" : "Còn hạn";
    }
}

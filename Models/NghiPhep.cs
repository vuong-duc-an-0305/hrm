using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    [Table("NghiPhep")]
    public class NghiPhep
    {
        [Key]
        public int MaNghiPhep { get; set; }

        [Required(ErrorMessage = "Nhân viên không được để trống")]
        [Display(Name = "Nhân viên")]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Từ ngày không được để trống")]
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime TuNgay { get; set; }

        [Required(ErrorMessage = "Đến ngày không được để trống")]
        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime DenNgay { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do")]
        public string? LyDo { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chờ duyệt";

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey(nameof(MaNhanVien))]
        public virtual NhanVien? NhanVien { get; set; }

        [NotMapped]
        [Display(Name = "Số ngày nghỉ")]
        public int SoNgayNghi
        {
            get
            {
                var from = TuNgay.Date;
                var to = DenNgay.Date;
                if (to < from)
                {
                    return 0;
                }

                return (to - from).Days + 1;
            }
        }
    }
}

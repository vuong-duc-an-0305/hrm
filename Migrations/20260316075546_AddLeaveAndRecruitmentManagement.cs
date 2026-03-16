using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveAndRecruitmentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NghiPhep",
                columns: table => new
                {
                    MaNghiPhep = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    TuNgay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DenNgay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NghiPhep", x => x.MaNghiPhep);
                    table.ForeignKey(
                        name: "FK_NghiPhep_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuyenDung",
                columns: table => new
                {
                    MaTuyenDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViTri = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaPhongBan = table.Column<int>(type: "int", nullable: true),
                    SoLuongCanTuyen = table.Column<int>(type: "int", nullable: false),
                    NgayDangTin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanNopHoSo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuyenDung", x => x.MaTuyenDung);
                    table.ForeignKey(
                        name: "FK_TuyenDung_PhongBan_MaPhongBan",
                        column: x => x.MaPhongBan,
                        principalTable: "PhongBan",
                        principalColumn: "MaPhongBan");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NghiPhep_MaNhanVien",
                table: "NghiPhep",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_TuyenDung_MaPhongBan",
                table: "TuyenDung",
                column: "MaPhongBan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NghiPhep");

            migrationBuilder.DropTable(
                name: "TuyenDung");
        }
    }
}

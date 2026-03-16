using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentScopeForManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaPhongBan",
                table: "NguoiDung",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "NguoiDung",
                keyColumn: "MaNguoiDung",
                keyValue: 1,
                column: "MaPhongBan",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaPhongBan",
                table: "NguoiDung",
                column: "MaPhongBan");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_PhongBan_MaPhongBan",
                table: "NguoiDung",
                column: "MaPhongBan",
                principalTable: "PhongBan",
                principalColumn: "MaPhongBan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_PhongBan_MaPhongBan",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "IX_NguoiDung_MaPhongBan",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "MaPhongBan",
                table: "NguoiDung");
        }
    }
}

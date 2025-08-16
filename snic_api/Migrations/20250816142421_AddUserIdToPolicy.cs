using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace snic_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_UserId",
                table: "Policies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Users_UserId",
                table: "Policies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Users_UserId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_UserId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Policies");
        }
    }
}

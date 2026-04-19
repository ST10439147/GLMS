using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10439147_PRO7311_POE.Migrations
{
    /// <inheritdoc />
    public partial class currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromCurrency",
                table: "ServiceRequests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToCurrency",
                table: "ServiceRequests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromCurrency",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ToCurrency",
                table: "ServiceRequests");
        }
    }
}

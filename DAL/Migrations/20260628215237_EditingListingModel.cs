using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditingListingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Listings",
                newName: "MinPrice");

            migrationBuilder.AddColumn<string>(
                name: "CertificateUrl",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "Listings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "MinPrice",
                table: "Listings",
                newName: "Price");
        }
    }
}

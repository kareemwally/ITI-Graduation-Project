using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedListingsFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellerCommissionShare",
                table: "Orders",
                newName: "PlatformCommission");

            migrationBuilder.RenameColumn(
                name: "BuyerTotalDue",
                table: "Orders",
                newName: "DownPaymentAmount");

            migrationBuilder.RenameColumn(
                name: "BuyerCommissionShare",
                table: "Orders",
                newName: "AgreedPricePerUnit");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryType",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "BuyerSignedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractGeneratedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractTerms",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractUrl",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DownPaymentPercentage",
                table: "Orders",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscrowReleaseAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisputed",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDownPaymentPaid",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignedByBuyer",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignedBySeller",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerSignedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Glass", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Paper / Cardboard", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Wood", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Rubber", null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "ParentId" },
                values: new object[,]
                {
                    { 9, "Leather", null },
                    { 10, "Electronics / E-Scrap", null },
                    { 11, "Construction / Demolition", null },
                    { 12, "Steel Scrap", 1 },
                    { 13, "Aluminium Scrap", 1 },
                    { 14, "PET", 2 },
                    { 15, "HDPE", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Listings_PublishedAt",
                table: "Listings",
                column: "PublishedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Listings_PublishedAt",
                table: "Listings");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DropColumn(
                name: "BuyerSignedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContractGeneratedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContractTerms",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContractUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DownPaymentPercentage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EscrowReleaseAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDisputed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDownPaymentPaid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSignedByBuyer",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSignedBySeller",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerSignedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Listings");

            migrationBuilder.RenameColumn(
                name: "PlatformCommission",
                table: "Orders",
                newName: "SellerCommissionShare");

            migrationBuilder.RenameColumn(
                name: "DownPaymentAmount",
                table: "Orders",
                newName: "BuyerTotalDue");

            migrationBuilder.RenameColumn(
                name: "AgreedPricePerUnit",
                table: "Orders",
                newName: "BuyerCommissionShare");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryType",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Steel Scrap", 1 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Aluminium Scrap", 1 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "PET", 2 });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "HDPE", 2 });
        }
    }
}

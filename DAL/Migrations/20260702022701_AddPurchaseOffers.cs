using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisputeId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    RaisedById = table.Column<int>(type: "int", nullable: false),
                    MediatorId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputes_AspNetUsers_MediatorId",
                        column: x => x.MediatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Disputes_AspNetUsers_RaisedById",
                        column: x => x.RaisedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disputes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuyerId1 = table.Column<int>(type: "int", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OfferedPricePerTon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyerMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOffers_AspNetUsers_BuyerId1",
                        column: x => x.BuyerId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOffers_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DisputeId",
                table: "Documents",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_MediatorId",
                table: "Disputes",
                column: "MediatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_OrderId",
                table: "Disputes",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_RaisedById",
                table: "Disputes",
                column: "RaisedById");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOffers_BuyerId1",
                table: "PurchaseOffers",
                column: "BuyerId1");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOffers_ListingId",
                table: "PurchaseOffers",
                column: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Disputes_DisputeId",
                table: "Documents",
                column: "DisputeId",
                principalTable: "Disputes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Disputes_DisputeId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "PurchaseOffers");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DisputeId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DisputeId",
                table: "Documents");
        }
    }
}

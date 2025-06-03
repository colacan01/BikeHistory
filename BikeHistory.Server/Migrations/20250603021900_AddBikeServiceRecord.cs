using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeHistory.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBikeServiceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BikeServiceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BikeFrameId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    ServiceDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceShopId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PartDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceStatus = table.Column<int>(type: "int", nullable: false),
                    NextServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikeServiceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BikeServiceRecords_AspNetUsers_ServiceShopId",
                        column: x => x.ServiceShopId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BikeServiceRecords_BikeFrames_BikeFrameId",
                        column: x => x.BikeFrameId,
                        principalTable: "BikeFrames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeServiceRecords_BikeFrameId",
                table: "BikeServiceRecords",
                column: "BikeFrameId");

            migrationBuilder.CreateIndex(
                name: "IX_BikeServiceRecords_ServiceShopId",
                table: "BikeServiceRecords",
                column: "ServiceShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BikeServiceRecords");
        }
    }
}

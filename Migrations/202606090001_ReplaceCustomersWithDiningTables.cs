using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringAnalyticsSystem.Migrations;

public partial class ReplaceCustomersWithDiningTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Customers",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_CustomerId",
            table: "Orders");

        migrationBuilder.CreateTable(
            name: "DiningTables",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Number = table.Column<int>(type: "int", nullable: false),
                SeatsCount = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiningTables", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DiningTables_Number",
            table: "DiningTables",
            column: "Number",
            unique: true);

        migrationBuilder.InsertData(
            table: "DiningTables",
            columns: new[] { "Number", "SeatsCount", "Status" },
            values: new object[,]
            {
                { 1, 2, "Free" },
                { 2, 2, "Free" },
                { 3, 4, "Free" },
                { 4, 4, "Free" },
                { 5, 6, "Free" },
                { 6, 6, "Reserved" }
            });

        migrationBuilder.AddColumn<int>(
            name: "DiningTableId",
            table: "Orders",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_DiningTableId",
            table: "Orders",
            column: "DiningTableId");

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_DiningTables_DiningTableId",
            table: "Orders",
            column: "DiningTableId",
            principalTable: "DiningTables",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.DropColumn(
            name: "CustomerId",
            table: "Orders");

        migrationBuilder.DropTable(
            name: "Customers");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Customers",
            columns: new[] { "FullName", "Phone", "Email" },
            values: new object[] { "Demo Customer", "", "" });

        migrationBuilder.AddColumn<int>(
            name: "CustomerId",
            table: "Orders",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerId",
            table: "Orders",
            column: "CustomerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Customers",
            table: "Orders",
            column: "CustomerId",
            principalTable: "Customers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.DropForeignKey(
            name: "FK_Orders_DiningTables_DiningTableId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_DiningTableId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "DiningTableId",
            table: "Orders");

        migrationBuilder.DropTable(
            name: "DiningTables");
    }
}

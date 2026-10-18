using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportEcommerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colors",
                table: "Products",
                type: "TEXT",
                nullable: true);

            // Products created before colours existed would otherwise return `colors: null`.
            migrationBuilder.Sql("UPDATE \"Products\" SET \"Colors\" = '[]' WHERE \"Colors\" IS NULL;");

            migrationBuilder.AddColumn<string>(
                name: "Sizes",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SoldOutSizes",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colors",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sizes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SoldOutSizes",
                table: "Products");
        }
    }
}

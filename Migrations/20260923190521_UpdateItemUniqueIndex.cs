using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Guid",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Id_Guid",
                table: "Items",
                columns: new[] { "Id", "Guid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Id_Guid",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Guid",
                table: "Items",
                column: "Guid",
                unique: true);
        }
    }
}

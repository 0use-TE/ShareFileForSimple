using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFileForSimple.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStoregeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StorageFileName",
                table: "FileItemModels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageFileName",
                table: "FileItemModels");
        }
    }
}

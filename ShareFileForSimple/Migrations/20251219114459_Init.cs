using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFileForSimple.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileRecordModels",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileDescription = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRecordModels", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "FileItemModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileRecordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileItemModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileItemModels_FileRecordModels_FileRecordId",
                        column: x => x.FileRecordId,
                        principalTable: "FileRecordModels",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileItemModels_FileRecordId",
                table: "FileItemModels",
                column: "FileRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileItemModels");

            migrationBuilder.DropTable(
                name: "FileRecordModels");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GotExplorer.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderboardRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Images",
                keyColumn: "Id",
                keyValue: new Guid("eaa88c90-9c31-4e69-aa13-7227f83d897d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e273ef6c-887e-45a2-9ed4-21b04286aca5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("eaa88c90-9c31-4e69-aa13-7227f83d897d"));

            migrationBuilder.CreateTable(
                name: "LeaderboardRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "Name", "Path" },
                values: new object[] { new Guid("e273ef6c-887e-45a2-9ed4-21b04286aca5"), "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRecords_UserId",
                table: "LeaderboardRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardRecords");

            migrationBuilder.DeleteData(
                table: "Images",
                keyColumn: "Id",
                keyValue: new Guid("e273ef6c-887e-45a2-9ed4-21b04286aca5"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("eaa88c90-9c31-4e69-aa13-7227f83d897d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e273ef6c-887e-45a2-9ed4-21b04286aca5"));

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "Name", "Path" },
                values: new object[] { new Guid("eaa88c90-9c31-4e69-aa13-7227f83d897d"), "", "" });
        }
    }
}

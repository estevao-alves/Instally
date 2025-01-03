using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstallyAPI.Migrations
{
    /// <inheritdoc />
    public partial class Migration01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserEntityGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Collections_Users_UserEntityGuid",
                        column: x => x.UserEntityGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    WingetId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", nullable: false),
                    TagsString = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Site = table.Column<string>(type: "TEXT", nullable: true),
                    VersionsLength = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    CollectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Packages_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Guid");
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Guid", "CreatedAt", "Email", "Password", "UpdatedAt" },
                values: new object[] { new Guid("f2e2774e-dc6d-4a5c-b2d6-0facbe6694bf"), new DateTime(2025, 1, 2, 20, 38, 17, 994, DateTimeKind.Local).AddTicks(9674), "user1@example.com", "password", new DateTime(2025, 1, 2, 20, 38, 18, 4, DateTimeKind.Local).AddTicks(4995) });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserEntityGuid",
                table: "Collections",
                column: "UserEntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_CollectionId",
                table: "Packages",
                column: "CollectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

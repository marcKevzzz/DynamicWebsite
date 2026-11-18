using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dcd.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeparatePortfolioSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [Content] WHERE [Section] NOT IN ('home','about','works','services','team','contacts')) THROW 50000, 'Unknown portfolio section. Review the rows before migration.', 1;");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_JoinedAt",
                table: "Users",
                newName: "IX_Users_JoinedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "About",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_About", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Home",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Home", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Works",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Works", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateIndex(
                name: "IX_About_IsOwner",
                table: "About",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsOwner",
                table: "Contacts",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Home_IsOwner",
                table: "Home",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Services_IsOwner",
                table: "Services",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Team_IsOwner",
                table: "Team",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Works_IsOwner",
                table: "Works",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            foreach (var table in Tables)
                migrationBuilder.Sql($"INSERT INTO [{table}] ({Columns}) SELECT {Columns} FROM [Content] WHERE [Section] = '{table.ToLowerInvariant()}'");
            migrationBuilder.DropTable("Content");
        }

        /// <inheritdoc />
        private static readonly string[] Tables = ["About", "Contacts", "Home", "Services", "Team", "Works"];
        private const string Columns = "[Section], [Key], [Position], [Payload], [IsOwner], [Revision]";

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "IX_Users_JoinedAt",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_JoinedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Content",
                columns: table => new
                {
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Revision = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content", x => new { x.Section, x.Key });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Content_IsOwner",
                table: "Content",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            foreach (var table in Tables)
            {
                migrationBuilder.Sql($"INSERT INTO [Content] ({Columns}) SELECT {Columns} FROM [{table}]");
                migrationBuilder.DropTable(table);
            }
        }
    }
}

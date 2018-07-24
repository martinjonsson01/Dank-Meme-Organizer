using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMO_Model.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaMetaJsons",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Json = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaMetaJsons", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Label",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Probability = table.Column<float>(nullable: false),
                    MediaMetaJsonID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Label", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Label_MediaMetaJsons_MediaMetaJsonID",
                        column: x => x.MediaMetaJsonID,
                        principalTable: "MediaMetaJsons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Label_MediaMetaJsonID",
                table: "Label",
                column: "MediaMetaJsonID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Label");

            migrationBuilder.DropTable(
                name: "MediaMetaJsons");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_Field_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subtitle_assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieId = table.Column<int>(type: "int", nullable: true),
                    EpisodeId = table.Column<int>(type: "int", nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bucket = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtitle_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subtitle_assets_episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "episodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_subtitle_assets_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_subtitle_assets_EpisodeId",
                table: "subtitle_assets",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_subtitle_assets_MovieId",
                table: "subtitle_assets",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subtitle_assets");
        }
    }
}

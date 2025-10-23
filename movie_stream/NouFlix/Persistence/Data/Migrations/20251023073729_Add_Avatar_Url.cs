using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Avatar_Url : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "image_vectors");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "ModelVersion",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "Phash",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "TsSeconds",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "image_assets");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subtitles_assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieId = table.Column<int>(type: "int", nullable: true),
                    EpisodeId = table.Column<int>(type: "int", nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bucket = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subtitles_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subtitles_assets_episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "episodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_subtitles_assets_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "movies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_subtitles_assets_EpisodeId",
                table: "subtitles_assets",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_subtitles_assets_MovieId",
                table: "subtitles_assets",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subtitles_assets");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "profiles");

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "image_assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelVersion",
                table: "image_assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Phash",
                table: "image_assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "image_assets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "image_assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TsSeconds",
                table: "image_assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "image_assets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "image_vectors",
                columns: table => new
                {
                    ImageAssetId = table.Column<int>(type: "int", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Embedding = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TsSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_vectors", x => new { x.ImageAssetId, x.ModelVersion });
                    table.ForeignKey(
                        name: "FK_image_vectors_image_assets_ImageAssetId",
                        column: x => x.ImageAssetId,
                        principalTable: "image_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_image_vectors_ModelVersion",
                table: "image_vectors",
                column: "ModelVersion");
        }
    }
}

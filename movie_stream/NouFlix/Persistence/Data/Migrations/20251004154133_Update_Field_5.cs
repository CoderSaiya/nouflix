using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Date.Migrations
{
    /// <inheritdoc />
    public partial class Update_Field_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "profiles");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "reviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "reviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "profiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileId",
                table: "image_assets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_AvatarId",
                table: "profiles",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_image_assets_ProfileId",
                table: "image_assets",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_image_assets_profiles_ProfileId",
                table: "image_assets",
                column: "ProfileId",
                principalTable: "profiles",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_image_assets_AvatarId",
                table: "profiles",
                column: "AvatarId",
                principalTable: "image_assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_image_assets_profiles_ProfileId",
                table: "image_assets");

            migrationBuilder.DropForeignKey(
                name: "FK_profiles_image_assets_AvatarId",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_AvatarId",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_image_assets_ProfileId",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "image_assets");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "profiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

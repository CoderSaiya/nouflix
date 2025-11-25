using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_History : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.RenameColumn(
            //     name: "Progress",
            //     table: "histories",
            //     newName: "PositionSecond");

            // migrationBuilder.AddColumn<int>(
            //     name: "EpisodeId",
            //     table: "histories",
            //     type: "int",
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<bool>(
            //     name: "IsCompleted",
            //     table: "histories",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_histories_EpisodeId",
            //     table: "histories",
            //     column: "EpisodeId");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_histories_episodes_EpisodeId",
            //     table: "histories",
            //     column: "EpisodeId",
            //     principalTable: "episodes",
            //     principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_histories_episodes_EpisodeId",
            //     table: "histories");

            // migrationBuilder.DropIndex(
            //     name: "IX_histories_EpisodeId",
            //     table: "histories");

            // migrationBuilder.DropColumn(
            //     name: "EpisodeId",
            //     table: "histories");

            // migrationBuilder.DropColumn(
            //     name: "IsCompleted",
            //     table: "histories");

            // migrationBuilder.RenameColumn(
            //     name: "PositionSecond",
            //     table: "histories",
            //     newName: "Progress");
        }
    }
}

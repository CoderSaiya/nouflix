using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyYearlyPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceMonthly",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceYearly",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PriceMonthly",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PriceYearly",
                table: "SubscriptionPlans");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NouFlix.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixesPlanName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SubscriptionPlans_PlanId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_users_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                table: "UserSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_users_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "transactions");

            migrationBuilder.RenameTable(
                name: "UserSubscriptions",
                newName: "user_subscriptions");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlans",
                newName: "subscription_plans");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "transactions",
                newName: "IX_transactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_PlanId",
                table: "transactions",
                newName: "IX_transactions_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "user_subscriptions",
                newName: "IX_user_subscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "user_subscriptions",
                newName: "IX_user_subscriptions_PlanId");

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "subscription_plans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactions",
                table: "transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_subscriptions",
                table: "user_subscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_subscription_plans",
                table: "subscription_plans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_subscription_plans_PlanId",
                table: "transactions",
                column: "PlanId",
                principalTable: "subscription_plans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_users_UserId",
                table: "transactions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_subscriptions_subscription_plans_PlanId",
                table: "user_subscriptions",
                column: "PlanId",
                principalTable: "subscription_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_subscriptions_users_UserId",
                table: "user_subscriptions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transactions_subscription_plans_PlanId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_users_UserId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_subscriptions_subscription_plans_PlanId",
                table: "user_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_user_subscriptions_users_UserId",
                table: "user_subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactions",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_subscriptions",
                table: "user_subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subscription_plans",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "subscription_plans");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "user_subscriptions",
                newName: "UserSubscriptions");

            migrationBuilder.RenameTable(
                name: "subscription_plans",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_PlanId",
                table: "Transactions",
                newName: "IX_Transactions_PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_user_subscriptions_UserId",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_user_subscriptions_PlanId",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_PlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SubscriptionPlans_PlanId",
                table: "Transactions",
                column: "PlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                table: "UserSubscriptions",
                column: "PlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_users_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Infrastructure.Server.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPeriodUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Budgets_OwnerId_PeriodYear_PeriodMonth",
                schema: "execution",
                table: "Budgets",
                columns: new[] { "OwnerId", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlans_OwnerId_PeriodYear_PeriodMonth",
                schema: "planning",
                table: "BudgetPlans",
                columns: new[] { "OwnerId", "PeriodYear", "PeriodMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_OwnerId_PeriodYear_PeriodMonth",
                schema: "execution",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_BudgetPlans_OwnerId_PeriodYear_PeriodMonth",
                schema: "planning",
                table: "BudgetPlans");
        }
    }
}

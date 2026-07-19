using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Infrastructure.Server.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetSourcePlanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceBudgetPlanId",
                schema: "execution",
                table: "Budgets",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE execution.\"Budgets\" SET \"SourceBudgetPlanId\" = \"Id\";");

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceBudgetPlanId",
                schema: "execution",
                table: "Budgets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SourceBudgetPlanId",
                schema: "execution",
                table: "Budgets",
                column: "SourceBudgetPlanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_SourceBudgetPlanId",
                schema: "execution",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "SourceBudgetPlanId",
                schema: "execution",
                table: "Budgets");
        }
    }
}

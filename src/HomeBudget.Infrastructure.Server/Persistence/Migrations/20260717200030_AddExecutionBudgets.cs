using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Infrastructure.Server.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "execution");

            migrationBuilder.CreateTable(
                name: "Budgets",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalIncomeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIncomeCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalExpensesAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalExpensesCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalSavingsAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSavingsCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ActualFinancialResultAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualFinancialResultCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OccurredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConvertedAmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ConvertedAmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ConversionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RemovalReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RemovedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "execution",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OccurredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConvertedAmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ConvertedAmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ConversionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RemovalReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RemovedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incomes_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "execution",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Savings",
                schema: "execution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OccurredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConvertedAmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ConvertedAmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ConversionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RemovalReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RemovedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Savings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Savings_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "execution",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetId",
                schema: "execution",
                table: "Expenses",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_BudgetId",
                schema: "execution",
                table: "Incomes",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Savings_BudgetId",
                schema: "execution",
                table: "Savings",
                column: "BudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses",
                schema: "execution");

            migrationBuilder.DropTable(
                name: "Incomes",
                schema: "execution");

            migrationBuilder.DropTable(
                name: "Savings",
                schema: "execution");

            migrationBuilder.DropTable(
                name: "Budgets",
                schema: "execution");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBudget.Infrastructure.Server.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.EnsureSchema(
                name: "planning");

            migrationBuilder.EnsureSchema(
                name: "outbox");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetPlans",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BudgetFitRisk = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalPlannedIncomeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPlannedIncomeCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAllocatedExpensesAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAllocatedExpensesCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalSavingContributionsAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSavingContributionsCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PlannedFinancialResultAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlannedFinancialResultCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategoryAllocations",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Flexibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpenseSharePercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    IncomeSharePercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    BudgetPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategoryAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategoryAllocations_BudgetPlans_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalSchema: "planning",
                        principalTable: "BudgetPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlannedIncomes",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConvertedAmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ConvertedAmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ConversionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BudgetPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedIncomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlannedIncomes_BudgetPlans_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalSchema: "planning",
                        principalTable: "BudgetPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingContributions",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BudgetPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingContributions_BudgetPlans_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalSchema: "planning",
                        principalTable: "BudgetPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_OwnerId_Name",
                schema: "shared",
                table: "BudgetCategories",
                columns: new[] { "OwnerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategoryAllocations_BudgetPlanId",
                schema: "planning",
                table: "ExpenseCategoryAllocations",
                column: "BudgetPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "outbox",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedIncomes_BudgetPlanId",
                schema: "planning",
                table: "PlannedIncomes",
                column: "BudgetPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingContributions_BudgetPlanId",
                schema: "planning",
                table: "SavingContributions",
                column: "BudgetPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Issuer_Subject",
                schema: "auth",
                table: "UserAccounts",
                columns: new[] { "Issuer", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_OwnerId",
                schema: "auth",
                table: "UserAccounts",
                column: "OwnerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetCategories",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "ExpenseCategoryAllocations",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "outbox");

            migrationBuilder.DropTable(
                name: "PlannedIncomes",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "SavingContributions",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "UserAccounts",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "BudgetPlans",
                schema: "planning");
        }
    }
}

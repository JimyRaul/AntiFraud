using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Antifraud.Infrastructure.Migrations;

[DbContext(typeof(Persistence.ApplicationDbContext))]
[Migration("20240101000000_CreateInitialTables")]
public class CreateInitialTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Crear schema
        migrationBuilder.EnsureSchema(name: "antifraud");

        // Tabla Accounts
        migrationBuilder.CreateTable(
            name: "accounts",
            schema: "antifraud",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_accounts", x => x.id);
            });

        // Tabla Transactions
        migrationBuilder.CreateTable(
            name: "transactions",
            schema: "antifraud",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                transfer_type_id = table.Column<int>(type: "integer", nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_transactions", x => x.id);
            });

        // Índices para Accounts
        migrationBuilder.CreateIndex(
            name: "ix_accounts_account_number",
            schema: "antifraud",
            table: "accounts",
            column: "account_number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_accounts_is_active",
            schema: "antifraud",
            table: "accounts",
            column: "is_active");

        // Índices para Transactions
        migrationBuilder.CreateIndex(
            name: "ix_transactions_source_account_id",
            schema: "antifraud",
            table: "transactions",
            column: "source_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_target_account_id",
            schema: "antifraud",
            table: "transactions",
            column: "target_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_created_at",
            schema: "antifraud",
            table: "transactions",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_status",
            schema: "antifraud",
            table: "transactions",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_source_account_created_at",
            schema: "antifraud",
            table: "transactions",
            columns: new[] { "source_account_id", "created_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "transactions",
            schema: "antifraud");

        migrationBuilder.DropTable(
            name: "accounts",
            schema: "antifraud");

        migrationBuilder.DropSchema(name: "antifraud");
    }
}
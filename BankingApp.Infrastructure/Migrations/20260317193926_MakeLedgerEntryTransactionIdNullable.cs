using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeLedgerEntryTransactionIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "LedgerEntries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Handle rows with NULL TransactionId by deleting them (or you could update to a valid ID if needed)
            // This ensures we don't violate FK constraints when converting back to non-nullable
            migrationBuilder.Sql("DELETE FROM \"LedgerEntries\" WHERE \"TransactionId\" IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "LedgerEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}

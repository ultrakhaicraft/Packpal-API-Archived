using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Packpal.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEntityRemoveRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add ActiveRole column
            migrationBuilder.AddColumn<string>(
                name: "ActiveRole",
                table: "Users",
                type: "text",
                nullable: true);

            // Step 2: Migrate existing data from Role to ActiveRole
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""ActiveRole"" = ""Role""
                WHERE ""ActiveRole"" IS NULL;
            ");

            // Step 3: Ensure Roles array has current Role value if empty
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""Roles"" = ARRAY[""Role""]
                WHERE ""Roles"" IS NULL OR array_length(""Roles"", 1) IS NULL;
            ");

            // Step 4: Set default value for ActiveRole where still null
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""ActiveRole"" = 'RENTER'
                WHERE ""ActiveRole"" IS NULL;
            ");

            // Step 5: Make ActiveRole NOT NULL with default value
            migrationBuilder.AlterColumn<string>(
                name: "ActiveRole",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "RENTER");

            // Step 6: Create indexes for better performance
            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveRole",
                table: "Users",
                column: "ActiveRole");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Roles",
                table: "Users",
                column: "Roles")
                .Annotation("Npgsql:IndexMethod", "GIN");

            // Step 7: Drop the old Role column (after data migration)
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveRole",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Roles",
                table: "Users");

            // Add back Role column
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "RENTER");

            // Migrate data back from ActiveRole to Role
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""Role"" = ""ActiveRole""
                WHERE ""Role"" IS NULL OR ""Role"" = '';
            ");

            // Drop ActiveRole column
            migrationBuilder.DropColumn(
                name: "ActiveRole",
                table: "Users");
        }
    }
}

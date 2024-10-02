using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P15.Migrations
{
    /// <inheritdoc />
    public partial class UserDeleteDt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDt",
                table: "Users");
        }
    }
}
